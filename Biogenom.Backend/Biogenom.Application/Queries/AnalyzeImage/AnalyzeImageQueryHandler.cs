using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.AnalyzeImage
{
    using AutoMapper;
    using Biogenom.Application.Queries.Image;
    using MediatR;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Biogenom.Application.Queries.Image
    {
        public class AnalyzeImageQueryHandler
          : IRequestHandler<AnalyzeImageQuery, AnalyzeImageResultVm>
        {
            private readonly IHttpClientFactory _httpClientFactory;

            public AnalyzeImageQueryHandler(IHttpClientFactory httpClientFactory)
            {
                _httpClientFactory = httpClientFactory;
            }

            public async Task<AnalyzeImageResultVm> Handle(
                AnalyzeImageQuery request,
                CancellationToken cancellationToken)
            {
                if (!File.Exists(request.FilePath))
                    throw new FileNotFoundException("Файл изображения не найден", request.FilePath);

                var token = Environment.GetEnvironmentVariable("GIGACHAT_BEARER_TOKEN");
                if (string.IsNullOrWhiteSpace(token))
                    throw new InvalidOperationException("GIGACHAT_BEARER_TOKEN не задан");

                var prompt = """
                Определи главный объект на изображении.

                Ответь строго в формате JSON без пояснений и без текста вне JSON:

                {
                  "objects": [
                    {
                      "name": "название объекта",
                      "confidence": 0.95
                    }
                  ]
                }

                confidence должен быть числом от 0 до 1.
                """;

                // ---- SSL отключен ----
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // ================= UPLOAD =================
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
                        cancellationToken);

                    var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);

                    if (!uploadResponse.IsSuccessStatusCode)
                        throw new HttpRequestException(
                            $"Ошибка загрузки файла: {uploadResponse.StatusCode} {uploadBody}");

                    using var uploadDoc = JsonDocument.Parse(uploadBody);
                    fileId = uploadDoc.RootElement.GetProperty("id").GetString()
                             ?? throw new Exception("fileId не получен");
                }

                // ================= ANALYZE =================
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

                var chatJson = JsonSerializer.Serialize(chatRequest);

                using var chatContent = new StringContent(
                    chatJson,
                    Encoding.UTF8,
                    "application/json");

                var chatResponse = await client.PostAsync(
                    "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                    chatContent,
                    cancellationToken);

                var chatBody = await chatResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!chatResponse.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"Ошибка анализа изображения: {chatResponse.StatusCode} {chatBody}");

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

                // Убираем возможный markdown ```json
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
                            Confidence = x.GetProperty("confidence").GetDecimal()
                        })
                        .ToList();
                }
                catch
                {
                    // fallback если модель нарушила формат
                    return new List<DetectedObjectVm>
                {
                    new DetectedObjectVm
                    {
                        Name = content,
                        Confidence = 1.0m
                    }
                };
                }
            }
        }
    }
}
