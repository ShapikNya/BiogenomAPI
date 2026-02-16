using Biogenom.Application.Commands.Image.Upload;
using Biogenom.Application.Queries.AnalyzeImage;
using Biogenom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Biogenom.Application.Commands.Image.ProcessImage
{
    public class ProcessImageCommandHandler : IRequestHandler<ProcessImageCommand, ProcessImageResultVm>
    {
        private readonly IBiogenomDbContext _dbContext;
        private readonly IMediator _mediator;

        public ProcessImageCommandHandler(IBiogenomDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<ProcessImageResultVm> Handle(ProcessImageCommand request, CancellationToken cancellationToken)
        {
            var imageId = await _mediator.Send(new UploadImageCommand { FileUrl = request.ImageUrl }, cancellationToken);

            var imageEntity = await _dbContext.Images
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == imageId, cancellationToken)
                ?? throw new Exception("Изображение не найдено");

            var absoluteFilePath = Path.Combine(AppContext.BaseDirectory, imageEntity.FilePath);

            if (!File.Exists(absoluteFilePath))
                throw new FileNotFoundException("Файл изображения не найден на сервере", absoluteFilePath);

            var analyzeResult = await _mediator.Send(new AnalyzeImageQuery { FilePath = absoluteFilePath }, cancellationToken);

            foreach (var detectedVm in analyzeResult.DetectedObjects)
            {
                var detectedEntity = new DetectedObject
                {
                    Id = Guid.NewGuid(),
                    ImageId = imageEntity.Id,
                    ObjectName = detectedVm.Name
                };

                await _dbContext.DetectedObjects.AddAsync(detectedEntity, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ProcessImageResultVm
            {
                DetectedObjects = analyzeResult.DetectedObjects
            };
        }
    }
}
