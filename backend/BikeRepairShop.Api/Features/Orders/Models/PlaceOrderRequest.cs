using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record PlaceOrderRequest(
    string CustomerName,
    string PhoneNumber,
    string Email,
    string BikeBrand,
    ServiceType ServiceType,
    DateOnly? DueDate,
    string? Notes);
