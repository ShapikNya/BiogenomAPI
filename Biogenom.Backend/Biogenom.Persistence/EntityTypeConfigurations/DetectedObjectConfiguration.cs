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
    [Table("DetectedObjects")]
    public class DetectedObjectConfiguration : IEntityTypeConfiguration<DetectedObject>
    {
        public void Configure(EntityTypeBuilder<DetectedObject> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.ObjectName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.ImageId)
                   .IsRequired();

            // Навигации
            builder.HasOne(x => x.Image)
                   .WithMany(i => i.DetectedObjects)
                   .HasForeignKey(x => x.ImageId);

            builder.HasMany(x => x.Materials)
                   .WithOne(m => m.DetectedObject)
                   .HasForeignKey(m => m.DetectedObjectId);
        }
    }
}
