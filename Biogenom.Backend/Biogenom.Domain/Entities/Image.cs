using Biogenom.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class Image
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } 
        public string Extension { get; set; } 
        public string FilePath { get; set; } 
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public ImageStatus Status { get; set; }

        // 1:M
        public List<DetectedObject> DetectedObjects { get; set; }
    }
}
