using Biogenom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Persistence.EntityTypeConfigurations
{
    [Table("ObjectMaterials")]
    public class ObjectMaterialConfiguration : IEntityTypeConfiguration<ObjectMaterial>
    {
        public void Configure(EntityTypeBuilder<ObjectMaterial> builder)
        {

            builder.HasKey(x => new { x.DetectedObjectId, x.MaterialName });

            builder.Property(x => x.MaterialName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Percentage)
                   .HasColumnType("decimal(5,2)")
                   .IsRequired();

            // Навигации
            builder.HasOne(x => x.DetectedObject)
                   .WithMany(d => d.Materials)
                   .HasForeignKey(x => x.DetectedObjectId);
        }
    }

}
