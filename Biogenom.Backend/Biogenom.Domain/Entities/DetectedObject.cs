using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class DetectedObject
    {
        public Guid Id { get; set; }
        public Guid ImageId { get; set; }
        public string ObjectName { get; set; } 

        // 1:M 
        public Image Image { get; set; }
        public List<ObjectMaterial> Materials { get; set; } 
    }
    
}
