using Biogenom.Application;
using Biogenom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Biogenom.Application.Commands.Image.Delete
{
    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Unit>
    {
        private readonly IBiogenomDbContext _dbContext;

        public DeleteImageCommandHandler(IBiogenomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            var image = await _dbContext.Images
                .Include(i => i.DetectedObjects)
                .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);

            if (image == null)
                throw new KeyNotFoundException($"Изображение с Id={request.ImageId} не найдено.");

            var uploadsFolder = Path.Combine(AppContext.BaseDirectory, "UploadedImages");
            var fileName = Path.GetFileName(image.FilePath);
            var fullPath = Path.Combine(uploadsFolder, fileName);

            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось удалить файл с диска: {ex.Message}");
            }

            _dbContext.Images.Remove(image);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
