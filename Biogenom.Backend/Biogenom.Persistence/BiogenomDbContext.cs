using Biogenom.Application;
using Biogenom.Domain.Entities;
using Biogenom.Persistence.EntityTypeConfigurations;
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

namespace Biogenom.Persistence
{
    public class BiogenomkDbContext : DbContext, IBiogenomDbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<DetectedObject> DetectedObjects { get; set; }
        public DbSet<ObjectMaterial> ObjectMaterials { get; set; }

        public BiogenomkDbContext(DbContextOptions<BiogenomkDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ImageConfiguration());
            builder.ApplyConfiguration(new DetectedObjectConfiguration());
            builder.ApplyConfiguration(new ObjectMaterialConfiguration());
            base.OnModelCreating(builder);
        }

    }
}
