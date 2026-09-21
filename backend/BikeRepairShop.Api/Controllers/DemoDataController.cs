using BikeRepairShop.Api.Features.DemoData;
using BikeRepairShop.Api.Features.DemoData.Handlers;
using BikeRepairShop.Api.Features.DemoData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Api.Controllers;

[ApiController]
[Route("api/demo-data")]
[Produces("application/json")]
public sealed class DemoDataController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<DemoDataStatus>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DemoDataStatus>> Status(
        [FromServices] GetDemoDataStatusHandler handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(cancellationToken));
    }

    [HttpPost("reset")]
    [ProducesResponseType<DemoDataStatus>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DemoDataStatus>> Reset(
        [FromServices] ResetDemoDataHandler handler,
        [FromServices] IOptions<DemoDataOptions> options,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return NotFound();
        }

        return Ok(DemoDataStatus.From(true, await handler.HandleAsync(cancellationToken)));
    }
}
