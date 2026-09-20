using BikeRepairShop.Domain;

namespace BikeRepairShop.Tests.Unit.Fixtures;

public sealed class FixedShopClock(DateTimeOffset now) : IShopClock
{
    public DateTimeOffset Now { get; } = now;

    public DateOnly Today { get; } = DateOnly.FromDateTime(now.DateTime);
}
