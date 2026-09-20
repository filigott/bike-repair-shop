using System.Diagnostics.CodeAnalysis;

namespace BikeRepairShop.Domain.Orders;

public sealed record PhoneNumber
{
    public const int MinDigits = 8;
    public const int MaxDigits = 15;
    public const int MaxLength = MaxDigits + 1;

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PhoneNumber Parse(string value)
    {
        var error = Validate(value, out var normalised);
        return error is null ? new PhoneNumber(normalised) : throw new ArgumentException(error, nameof(value));
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out PhoneNumber? result)
    {
        result = Validate(value, out var normalised) is null ? new PhoneNumber(normalised) : null;
        return result is not null;
    }

    public override string ToString()
    {
        return Value;
    }

    private static string? Validate(string? value, out string normalised)
    {
        normalised = value?.Replace(" ", "") ?? "";
        if (normalised.Length == 0)
        {
            return "A phone number is required.";
        }

        var digits = normalised.StartsWith('+') ? normalised[1..] : normalised;
        if (digits.Length is < MinDigits or > MaxDigits || !digits.All(char.IsAsciiDigit))
        {
            return $"A phone number is {MinDigits} to {MaxDigits} digits with an optional leading '+'.";
        }

        return null;
    }
}
