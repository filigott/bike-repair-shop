using BikeRepairShop.Domain;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Api.Configuration;

public sealed class ShopClock(TimeProvider timeProvider, IOptions<ShopOptions> options) : IShopClock
{
    private readonly TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    // Kept in UTC: Npgsql writes timestamptz only from a zero offset.
    public DateTimeOffset Now => timeProvider.GetUtcNow();

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Now, timeZone).DateTime);
}
