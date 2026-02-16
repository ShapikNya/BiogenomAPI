using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Queries.AnalyzeMaterials
{
    public class AnalyzeMaterialsResultVm
    {
        public Guid ImageId { get; set; } 
        public List<MaterialVm> Materials { get; set; } = new List<MaterialVm>();
    }
}
