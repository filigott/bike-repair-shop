namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record RescheduleOrderRequest(DateOnly? DueDate, uint? Version);
