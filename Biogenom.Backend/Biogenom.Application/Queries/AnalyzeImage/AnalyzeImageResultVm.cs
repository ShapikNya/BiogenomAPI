using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.AnalyzeImage
{
    public class AnalyzeImageResultVm
    {
        public List<DetectedObjectVm> DetectedObjects { get; set; } = new List<DetectedObjectVm>();
    }
}
