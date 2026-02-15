using Biogenom.Application;
using Biogenom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
                throw new KeyNotFoundException("Изображение не найдено.");

            // Удаляем файл с диска
            var fullPath = Path.Combine(AppContext.BaseDirectory, image.FilePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Удаляем запись из БД (включая связанные DetectedObjects)
            _dbContext.Images.Remove(image);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}