using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Handlers;
using BikeRepairShop.Api.Features.Orders.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Place(
        [FromServices] PlaceOrderHandler handler,
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(PlaceOrderCommand.From(request), cancellationToken);
        // TODO (#2): CreatedAtAction once get-by-id exists to point the Location header at.
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
