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
          @"
Определи все значимые объекты на изображении.

Ответь строго в формате JSON без объяснений и текста вне JSON.

JSON должен содержать массив ""objects"", каждый объект которого имеет поле:
- ""name"": название объекта (строка)

Пример формата:

{
  ""objects"": [
    { ""name"": ""красное яблоко"" },
    { ""name"": ""зелёный лист"" }
  ]
}

Выведи все объекты, которые можешь определить, не ограничиваясь одним. 
";
    }
}
