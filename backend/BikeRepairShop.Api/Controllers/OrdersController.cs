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

    [HttpGet("{id:guid}")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Get(
        [FromServices] GetOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetOrderQuery(id), cancellationToken);
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
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }
}
