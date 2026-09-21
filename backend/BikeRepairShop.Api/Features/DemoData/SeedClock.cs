using BikeRepairShop.Domain;

namespace BikeRepairShop.Api.Features.DemoData;

// Places a demo order as of a past day, so its due date was still ahead of it at the time.
internal sealed record SeedClock(DateTimeOffset Now, DateOnly Today) : IShopClock;
