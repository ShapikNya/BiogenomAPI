using Biogenom.Domain.Entities;
using Biogenom.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands.Image.Upload
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, Guid>
    {
        private readonly IBiogenomDbContext _dbContext;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public UploadImageCommandHandler(IBiogenomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            byte[] bytes;
            try
            {
                bytes = await httpClient.GetByteArrayAsync(request.FileUrl, cancellationToken);
            }
            catch
            {
                throw new ArgumentException("Не удалось скачать файл по указанной ссылке.");
            }

            var extension = Path.GetExtension(request.FileUrl).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Неподдерживаемый формат файла: {extension}");

            var uploadsFolder = Path.Combine(AppContext.BaseDirectory, "UploadedImages");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);

            var image = new Biogenom.Domain.Entities.Image
            {
                Id = Guid.NewGuid(),
                FilePath = Path.Combine("UploadedImages", fileName),
                Extension = extension,
                FileSize = bytes.Length,
                Status = ImageStatus.Uploaded,
                CreatedAt = DateTime.UtcNow,
                DetectedObjects = new List<DetectedObject>()
            };

            await _dbContext.Images.AddAsync(image, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return image.Id;
        }
    }
}
