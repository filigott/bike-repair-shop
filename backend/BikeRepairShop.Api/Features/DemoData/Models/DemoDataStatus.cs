namespace BikeRepairShop.Api.Features.DemoData.Models;

public sealed record DemoDataStatus(bool Enabled, int OrderCount)
{
    public static DemoDataStatus From(bool enabled, int orderCount)
    {
        return new DemoDataStatus(enabled, orderCount);
    }
}
