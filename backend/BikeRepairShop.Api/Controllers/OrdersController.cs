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

    [HttpPut("{id:guid}/details")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MaintenanceOrderResponse>> CorrectDetails(
        [FromServices] CorrectOrderDetailsHandler handler,
        Guid id,
        CorrectOrderDetailsRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(CorrectOrderDetailsCommand.From(id, request), cancellationToken);
    }

    [HttpPut("{id:guid}/service")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MaintenanceOrderResponse>> ChangeService(
        [FromServices] ChangeOrderServiceHandler handler,
        Guid id,
        ChangeOrderServiceRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(ChangeOrderServiceCommand.From(id, request), cancellationToken);
    }

    [HttpPut("{id:guid}/due-date")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Reschedule(
        [FromServices] RescheduleOrderHandler handler,
        Guid id,
        RescheduleOrderRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(RescheduleOrderCommand.From(id, request), cancellationToken);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Start(
        [FromServices] StartOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new StartOrderCommand(id), cancellationToken);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Complete(
        [FromServices] CompleteOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new CompleteOrderCommand(id), cancellationToken);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Cancel(
        [FromServices] CancelOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new CancelOrderCommand(id), cancellationToken);
    }

    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType<MaintenanceOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MaintenanceOrderResponse>> Reopen(
        [FromServices] ReopenOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new ReopenOrderCommand(id), cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteOrderHandler handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeleteOrderCommand(id), cancellationToken);
        return NoContent();
    }
}
