using BikeRepairShop.Domain;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class CancellingShopClock(IShopClock inner, CancellationTokenSource cancellation) : IShopClock
{
    public DateTimeOffset Now => inner.Now;

    public DateOnly Today
    {
        get
        {
            cancellation.Cancel();
            return inner.Today;
        }
    }
}
