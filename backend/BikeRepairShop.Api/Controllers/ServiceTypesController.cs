using BikeRepairShop.Api.Features.ServiceTypes.Handlers;
using BikeRepairShop.Api.Features.ServiceTypes.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Controllers;

[ApiController]
[Route("api/service-types")]
[Produces("application/json")]
public sealed class ServiceTypesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceTypeItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceTypeItem>>> List(
        [FromServices] ListServiceTypesHandler handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(cancellationToken));
    }
}
