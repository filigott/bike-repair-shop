namespace BikeRepairShop.Domain.Orders;

public sealed record EmailAddress
{
    public const int MaxLength = 254;

    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalised = value.Trim().ToLowerInvariant();
        if (normalised.Length > MaxLength)
        {
            throw new ArgumentException($"An email address cannot exceed {MaxLength} characters.", nameof(value));
        }

        var at = normalised.IndexOf('@');
        if (at <= 0 || at != normalised.LastIndexOf('@') || normalised.Contains(' '))
        {
            throw new ArgumentException("An email address needs one '@' with a local part before it.", nameof(value));
        }

        var domain = normalised[(at + 1)..];
        var dot = domain.IndexOf('.');
        if (dot <= 0 || domain.EndsWith('.'))
        {
            throw new ArgumentException("An email address needs a domain with a dot in it.", nameof(value));
        }

        return new EmailAddress(normalised);
    }

    public override string ToString()
    {
        return Value;
    }
}
