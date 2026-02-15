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

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadImageCommand command)
    {
        var imageId = await _mediator.Send(command);
        return Ok(imageId);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteImageCommand { ImageId = id });
        return NoContent();
    }
}
