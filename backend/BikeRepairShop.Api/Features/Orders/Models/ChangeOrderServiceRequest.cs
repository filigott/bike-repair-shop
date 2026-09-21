using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record ChangeOrderServiceRequest(ServiceType ServiceType);
