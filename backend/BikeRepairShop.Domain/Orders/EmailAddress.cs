using System.Diagnostics.CodeAnalysis;

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
        var error = Validate(value, out var normalised);
        return error is null ? new EmailAddress(normalised) : throw new ArgumentException(error, nameof(value));
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out EmailAddress? result)
    {
        result = Validate(value, out var normalised) is null ? new EmailAddress(normalised) : null;
        return result is not null;
    }

    public override string ToString()
    {
        return Value;
    }

    private static string? Validate(string? value, out string normalised)
    {
        normalised = value?.Trim().ToLowerInvariant() ?? "";
        if (normalised.Length == 0)
        {
            return "An email address is required.";
        }

        if (normalised.Length > MaxLength)
        {
            return $"An email address cannot exceed {MaxLength} characters.";
        }

        var at = normalised.IndexOf('@');
        if (at <= 0 || at != normalised.LastIndexOf('@') || normalised.Contains(' '))
        {
            return "An email address needs one '@' with a local part before it.";
        }

        var domain = normalised[(at + 1)..];
        var dot = domain.IndexOf('.');
        if (dot <= 0 || domain.EndsWith('.'))
        {
            return "An email address needs a domain with a dot in it.";
        }

        return null;
    }
}
