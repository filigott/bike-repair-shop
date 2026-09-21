namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class TestTimeProvider : TimeProvider
{
    public static readonly DateTimeOffset Start = new(2026, 3, 16, 8, 0, 0, TimeSpan.Zero);

    private DateTimeOffset now = Start;

    public override DateTimeOffset GetUtcNow()
    {
        return now;
    }

    public void Advance(TimeSpan delta)
    {
        now += delta;
    }

    public void Reset()
    {
        now = Start;
    }
}
