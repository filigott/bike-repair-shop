namespace BikeRepairShop.Domain;

public interface IShopClock
{
    DateTimeOffset Now { get; }

    // The shop's calendar day, not the server's.
    DateOnly Today { get; }
}
