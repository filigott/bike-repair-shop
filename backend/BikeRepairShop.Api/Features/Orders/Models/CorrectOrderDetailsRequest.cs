namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record CorrectOrderDetailsRequest(
    string CustomerName,
    string PhoneNumber,
    string Email,
    string BikeBrand,
    string? Notes,
    uint? Version);
