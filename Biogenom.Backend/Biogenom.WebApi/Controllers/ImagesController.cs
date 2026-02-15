using Biogenom.Application.Commands.Image.Upload;
using Biogenom.Application.Commands.Image.Delete;
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
}
