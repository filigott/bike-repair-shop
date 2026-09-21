using BikeRepairShop.Domain;

namespace BikeRepairShop.Api.Features.DemoData;

// Place refuses a due date before today, so a back-dated order needs a back-dated today.
internal sealed record SeedClock(DateTimeOffset Now, DateOnly Today) : IShopClock;
