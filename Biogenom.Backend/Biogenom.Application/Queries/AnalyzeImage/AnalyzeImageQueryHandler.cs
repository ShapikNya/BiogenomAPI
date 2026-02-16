using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Biogenom.Application.Queries.AnalyzeImage
{
    public class AnalyzeImageQueryHandler : IRequestHandler<AnalyzeImageQuery, AnalyzeImageResultVm>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AnalyzeImageQueryHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AnalyzeImageResultVm> Handle(AnalyzeImageQuery request, CancellationToken cancellationToken)
        {
            if (!File.Exists(request.FilePath))
                throw new FileNotFoundException("Файл изображения не найден", request.FilePath);

            var token = Environment.GetEnvironmentVariable("GIGACHAT_BEARER_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("GIGACHAT_BEARER_TOKEN не задан");

            // Используем промт из запроса или дефолтный
            var prompt = string.IsNullOrWhiteSpace(request.Prompt)
                ? AnalyzeImageQuery.DefaultPrompt
                : request.Prompt;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/114.0.0.0 Safari/537.36"
            );

            // Загружаем изображение
            string fileId;
            using (var fileStream = File.OpenRead(request.FilePath))
            using (var multipart = new MultipartFormDataContent())
            {
                var fileName = Path.GetFileName(request.FilePath);
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

            // Отправляем промт в GigaChat
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
                throw new HttpRequestException($"Ошибка анализа изображения: {chatResponse.StatusCode} {chatBody}");

            using var chatDoc = JsonDocument.Parse(chatBody);
            var contentText = chatDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var detectedObjects = ParseModelResponse(contentText);

            return new AnalyzeImageResultVm
            {
                DetectedObjects = detectedObjects
            };
        }

        private static List<DetectedObjectVm> ParseModelResponse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<DetectedObjectVm>();

            content = content.Trim();
            if (content.StartsWith("```"))
            {
                var start = content.IndexOf('{');
                var end = content.LastIndexOf('}');
                if (start >= 0 && end > start)
                    content = content.Substring(start, end - start + 1);
            }

            try
            {
                using var doc = JsonDocument.Parse(content);
                return doc.RootElement
                    .GetProperty("objects")
                    .EnumerateArray()
                    .Select(x => new DetectedObjectVm
                    {
                        Name = x.GetProperty("name").GetString() ?? "",
                    })
                    .ToList();
            }
            catch
            {
                return new List<DetectedObjectVm>
                {
                    new DetectedObjectVm
                    {
                        Name = content
                    }
                };
            }
        }
    }
}
