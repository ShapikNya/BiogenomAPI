using Biogenom.Application.Commands.Image.Delete;
using Biogenom.Application.Commands.Image.ProcessImage;
using Biogenom.Application.Commands.Image.Upload;
using Biogenom.Application.Queries.AnalyzeImage;
using Biogenom.Application.Queries.AnalyzeMaterials;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/image")]
public class ImagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Uploads an image from a given URL.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /api/image/upload
    /// {
    ///     "FileUrl": "https://example.com/image.png"
    /// }
    /// </remarks>
    /// <param name="command">Command containing the URL of the image to upload</param>
    /// <returns>The ID of the uploaded image</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadImageCommand command)
    {
        var imageId = await _mediator.Send(command);
        return Ok(imageId);
    }


    /// <summary>
    /// Deletes an image by its ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/image/{id}
    /// </remarks>
    /// <param name="id">The ID of the image to delete</param>
    /// <returns>No content if the deletion is successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteImageCommand { ImageId = id });
        return NoContent();
    }

    /// <summary>
    /// Analyzes an image via GigaChat API.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /api/image/analyze
    /// {
    ///     "FilePath": "C:\\images\\apple.jpg",
    ///     "Prompt": "Опциональный промт"
    /// }
    /// </remarks>
    /// <param name="query">Query containing the local file path and optional prompt</param>
    /// <returns>List of detected objects on the image</returns>
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeImageQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Uploads an image by URL, analyzes it, and returns detected objects.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /api/image/process
    /// {
    ///     "ImageUrl": "https://example.com/image.jpg"
    /// }
    /// </remarks>
    /// <param name="command">Command containing the URL of the image</param>
    /// <returns>The ID of the uploaded image and detected objects</returns>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessImageCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Analyzes materials of user-confirmed objects on an uploaded image and returns the list of objects with their materials.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /api/images/{id}/analyze-materials
    /// [
    ///     "ручка",
    ///     "ежедневник"
    /// ]
    /// </remarks>
    /// <param name="id">ID of the uploaded image</param>
    /// <param name="objectNames">List of object names confirmed/corrected by the user</param>
    /// <returns>The ID of the image and a list of objects with their corresponding materials</returns>
    [HttpPost("{id}/analyze-materials")]
    public async Task<IActionResult> AnalyzeMaterials(Guid id, [FromBody] List<string> objectNames)
    {
        var command = new AnalyzeMaterialsQuery
        {
            ImageId = id,
            ObjectNames = objectNames
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
