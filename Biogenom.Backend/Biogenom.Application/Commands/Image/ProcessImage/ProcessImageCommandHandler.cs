using Biogenom.Application.Commands.Image.Upload;
using Biogenom.Application.Queries.AnalyzeImage;
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
            // 1. Сначала загружаем изображение через UploadImageCommand
            var imageId = await _mediator.Send(new UploadImageCommand { FileUrl = request.ImageUrl }, cancellationToken);

            // 2. Получаем сущность изображения из БД
            var imageEntity = await _dbContext.Images
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == imageId, cancellationToken)
                ?? throw new Exception("Изображение не найдено");

            // 3. Строим абсолютный путь к файлу для AnalyzeImageQuery
            var absoluteFilePath = Path.Combine(AppContext.BaseDirectory, imageEntity.FilePath);

            if (!File.Exists(absoluteFilePath))
                throw new FileNotFoundException("Файл изображения не найден на сервере", absoluteFilePath);

            // 4. Отправляем хендлеру анализа изображения
            var analyzeResult = await _mediator.Send(new AnalyzeImageQuery { FilePath = absoluteFilePath }, cancellationToken);

            // 5. Формируем результат
            return new ProcessImageResultVm
            {
                DetectedObjects = analyzeResult.DetectedObjects
            };
        }
    }
}
