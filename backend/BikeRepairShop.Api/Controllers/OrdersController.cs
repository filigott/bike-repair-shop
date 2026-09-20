using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Handlers;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Features.Orders.Queries;
using BikeRepairShop.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<MaintenanceOrderSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<MaintenanceOrderSummary>>> List(
        [FromServices] ListOrdersHandler handler,
        [FromQuery] ListOrdersRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(ListOrdersQuery.From(request), cancellationToken);
    }

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
