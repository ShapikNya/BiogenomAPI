using MediatR;
using System;
using System.Collections.Generic;

namespace Biogenom.Application.Queries.AnalyzeMaterials
{
    public class AnalyzeMaterialsQuery : IRequest<AnalyzeMaterialsResultVm>
    {
        public Guid ImageId { get; set; }
        public List<string> ObjectNames { get; set; } = new List<string>();

        public string DefaultPrompt => @"
            Определи, из каких материалов сделаны следующие объекты на изображении: {objects}.

            Ответь строго в формате JSON без текста вне JSON:

            {
              ""materials"": [
                { ""object"": ""название объекта"", ""material"": ""материал"" }
              ]
            }

            Выведи все объекты из переданного массива.
            ";
    }
}
