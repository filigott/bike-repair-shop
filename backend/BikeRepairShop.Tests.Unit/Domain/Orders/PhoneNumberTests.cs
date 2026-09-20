using AwesomeAssertions;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Unit.Domain.Orders;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("91234567", "91234567")]
    [InlineData("+47 912 34 567", "+4791234567")]
    [InlineData(" +4791234567 ", "+4791234567")]
    [InlineData("+1 415 555 0123", "+14155550123")]
    public void Parse_normalises_a_valid_number(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Parse(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")]
    [InlineData("+1234567890123456")]
    [InlineData("9123 45 67a")]
    [InlineData("912-34-567")]
    [InlineData("++4791234567")]
    [InlineData("+")]
    public void Parse_rejects_a_malformed_number(string input)
    {
        // Act
        var act = () => PhoneNumber.Parse(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryParse_returns_the_normalised_number_when_valid()
    {
        // Act
        var ok = PhoneNumber.TryParse("+47 912 34 567", out var result);

        // Assert
        ok.Should().BeTrue();
        result!.Value.Should().Be("+4791234567");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("912-34-567")]
    public void TryParse_returns_false_without_throwing_when_malformed(string? input)
    {
        // Act
        var ok = PhoneNumber.TryParse(input, out var result);

        // Assert
        ok.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void Equality_is_by_value()
    {
        // Arrange
        var left = PhoneNumber.Parse("+47 912 34 567");
        var right = PhoneNumber.Parse("+4791234567");

        // Act
        var equal = left == right;

        // Assert
        equal.Should().BeTrue();
    }
}
