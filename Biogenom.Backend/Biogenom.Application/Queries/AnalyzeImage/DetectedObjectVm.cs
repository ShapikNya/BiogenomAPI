using AutoMapper;
using Biogenom.Application.Common.Mappings;
using Biogenom.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.AnalyzeImage
{
    public class DetectedObjectVm : IMapWith<DetectedObject>
    {
        public string Name { get; set; }
        public decimal Confidence { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DetectedObject, DetectedObjectVm>();
        }
    }
}
