using Biogenom.Domain.Entities;
using Biogenom.Domain.Enums;
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
    [Table("Images")]
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.Extension)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(x => x.FileSize)
                   .IsRequired();

            builder.Property(x => x.FilePath)
                   .IsRequired()
                   .HasMaxLength(1024);

            builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(16)
                .HasDefaultValue(ImageStatus.Uploaded)
               .IsRequired();

            builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("NOW()") 
               .HasColumnType("date")
               .ValueGeneratedOnAdd();

            // Навигации
            builder.HasMany(x => x.DetectedObjects)
                   .WithOne(d => d.Image)
                   .HasForeignKey(d => d.ImageId);
        }
    }
}
