using Biogenom.Application.Queries.AnalyzeImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands.Image.ProcessImage
{
    public class ProcessImageResultVm
    {
        public List<DetectedObjectVm> DetectedObjects { get; set; } = new();
    }
}
