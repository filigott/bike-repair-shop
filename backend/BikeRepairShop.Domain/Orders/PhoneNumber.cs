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
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalised = value.Replace(" ", "");
        var digits = normalised.StartsWith('+') ? normalised[1..] : normalised;
        if (digits.Length is < MinDigits or > MaxDigits || !digits.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                $"A phone number is {MinDigits} to {MaxDigits} digits with an optional leading '+'.",
                nameof(value));
        }

        return new PhoneNumber(normalised);
    }

    public override string ToString()
    {
        return Value;
    }
}
