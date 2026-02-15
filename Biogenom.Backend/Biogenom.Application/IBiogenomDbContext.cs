using Biogenom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Biogenom.Application
{
    public interface IBiogenomDbContext
    {
        DbSet<Image> Images { get; set; }
        DbSet<DetectedObject> DetectedObjects { get; set; }
        DbSet<ObjectMaterial> ObjectMaterials { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
