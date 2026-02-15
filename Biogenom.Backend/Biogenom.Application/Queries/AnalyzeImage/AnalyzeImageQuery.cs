using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.AnalyzeImage
{
    public class AnalyzeImageQuery : IRequest<AnalyzeImageResultVm>
    {
        public string FilePath { get; set; }
        public string? Prompt { get; set; }

        public const string DefaultPrompt =
            """
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
    }
}
