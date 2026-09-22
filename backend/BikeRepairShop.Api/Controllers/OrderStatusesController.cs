using BikeRepairShop.Api.Features.OrderStatuses.Handlers;
using BikeRepairShop.Api.Features.OrderStatuses.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Controllers;

[ApiController]
[Route("api/order-statuses")]
[Produces("application/json")]
public sealed class OrderStatusesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderStatusItem>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OrderStatusItem>> List(
        [FromServices] ListOrderStatusesHandler handler)
    {
        return Ok(handler.Handle());
    }
}
