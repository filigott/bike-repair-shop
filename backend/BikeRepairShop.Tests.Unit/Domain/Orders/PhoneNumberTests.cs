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
        // Arrange

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
        // Arrange

        // Act
        var act = () => PhoneNumber.Parse(input);

        // Assert
        act.Should().Throw<ArgumentException>();
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
