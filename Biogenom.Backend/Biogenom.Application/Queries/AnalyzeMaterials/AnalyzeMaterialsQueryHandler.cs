using Biogenom.Application.Queries.AnalyzeMaterials;
using Biogenom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.Image
{
    public class AnalyzeMaterialsQueryHandler : IRequestHandler<AnalyzeMaterialsQuery, AnalyzeMaterialsResultVm>
    {
        private readonly IBiogenomDbContext _dbContext;

        public AnalyzeMaterialsQueryHandler(IBiogenomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AnalyzeMaterialsResultVm> Handle(AnalyzeMaterialsQuery request, CancellationToken cancellationToken)
        {
            var imageEntity = await _dbContext.Images
                .Include(x => x.DetectedObjects)
                .Include(x => x.DetectedObjects)
                  .ThenInclude(d => d.Materials)
                .FirstOrDefaultAsync(x => x.Id == request.ImageId, cancellationToken)
                ?? throw new FileNotFoundException("Изображение не найдено в базе");

            var absoluteFilePath = Path.Combine(AppContext.BaseDirectory, imageEntity.FilePath);
            if (!File.Exists(absoluteFilePath))
                throw new FileNotFoundException("Файл изображения не найден на сервере", absoluteFilePath);

            if (request.ObjectNames != null && request.ObjectNames.Any())
            {
                var existingObjects = imageEntity.DetectedObjects.ToList();
                _dbContext.ObjectMaterials.RemoveRange(existingObjects.SelectMany(o => o.Materials));
                _dbContext.DetectedObjects.RemoveRange(existingObjects);
                await _dbContext.SaveChangesAsync(cancellationToken);

                foreach (var name in request.ObjectNames)
                {
                    var newObj = new DetectedObject
                    {
                        ImageId = imageEntity.Id,
                        ObjectName = name
                    };
                    await _dbContext.DetectedObjects.AddAsync(newObj, cancellationToken);
                    imageEntity.DetectedObjects.Add(newObj);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var objectsForPrompt = imageEntity.DetectedObjects.Select(d => d.ObjectName).ToList();
            if (!objectsForPrompt.Any())
                objectsForPrompt.Add("main object");

            var prompt = request.DefaultPrompt.Replace("{objects}", string.Join(", ", objectsForPrompt));

            var token = Environment.GetEnvironmentVariable("GIGACHAT_BEARER_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Токен GigaChat не задан в env");

            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/114.0.0.0 Safari/537.36"
            );

            string fileId;
            using (var fileStream = File.OpenRead(absoluteFilePath))
            using (var multipart = new MultipartFormDataContent())
            {
                var fileName = Path.GetFileName(absoluteFilePath);
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => throw new NotSupportedException($"Формат {extension} не поддерживается")
                };

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                multipart.Add(fileContent, "file", fileName);
                multipart.Add(new StringContent("general"), "purpose");

                var uploadResponse = await client.PostAsync(
                    "https://gigachat.devices.sberbank.ru/api/v1/files",
                    multipart,
                    cancellationToken
                );

                var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
                if (!uploadResponse.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка загрузки файла: {uploadResponse.StatusCode} {uploadBody}");

                using var uploadDoc = JsonDocument.Parse(uploadBody);
                fileId = uploadDoc.RootElement.GetProperty("id").GetString()
                         ?? throw new Exception("fileId не получен");
            }

            var chatRequest = new
            {
                model = "GigaChat-Pro",
                function_call = "auto",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt,
                        attachments = new[] { fileId }
                    }
                },
                stream = false,
                update_interval = 0
            };

            var chatContent = new StringContent(
                JsonSerializer.Serialize(chatRequest),
                Encoding.UTF8,
                "application/json"
            );

            var chatResponse = await client.PostAsync(
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                chatContent,
                cancellationToken
            );

            var chatBody = await chatResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!chatResponse.IsSuccessStatusCode)
                throw new HttpRequestException($"Ошибка анализа материалов: {chatResponse.StatusCode} {chatBody}");

            using var chatDoc = JsonDocument.Parse(chatBody);
            var contentText = chatDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var resultVm = new AnalyzeMaterialsResultVm { ImageId = imageEntity.Id };

            try
            {
                using var jsonDoc = JsonDocument.Parse(contentText);
                if (jsonDoc.RootElement.TryGetProperty("materials", out var materialsElement))
                {
                    foreach (var obj in materialsElement.EnumerateArray())
                    {
                        var objectName = obj.GetProperty("object").GetString() ?? "";
                        var materialName = obj.GetProperty("material").GetString() ?? "";

                        var detectedObjectEntity = imageEntity.DetectedObjects
                            .FirstOrDefault(d => string.Equals(d.ObjectName, objectName, StringComparison.OrdinalIgnoreCase));

                        if (detectedObjectEntity != null)
                        {
                            var existingMaterial = detectedObjectEntity.Materials.FirstOrDefault();
                            if (existingMaterial != null)
                            {
                                existingMaterial.MaterialName = materialName;
                            }
                            else
                            {
                                var materialEntity = new ObjectMaterial
                                {
                                    DetectedObjectId = detectedObjectEntity.Id,
                                    MaterialName = materialName
                                };
                                await _dbContext.ObjectMaterials.AddAsync(materialEntity, cancellationToken);
                            }
                        }

                        resultVm.Materials.Add(new MaterialVm
                        {
                            ObjectName = objectName,
                            MaterialName = materialName
                        });
                    }
                }
            }
            catch
            {
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return resultVm;
        }
    }
}
