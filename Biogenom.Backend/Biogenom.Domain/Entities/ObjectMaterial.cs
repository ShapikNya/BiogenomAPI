using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class ObjectMaterial
    {
        public Guid DetectedObjectId { get; set; }
        public string MaterialName { get; set; } 
        public decimal Percentage { get; set; }

        // 1 : M
        public DetectedObject DetectedObject { get; set; } 
    }
}
