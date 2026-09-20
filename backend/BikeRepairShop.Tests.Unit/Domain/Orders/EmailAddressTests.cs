using AwesomeAssertions;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Unit.Domain.Orders;

public class EmailAddressTests
{
    [Theory]
    [InlineData("kari@example.com", "kari@example.com")]
    [InlineData("  Kari.Nordmann@Example.COM ", "kari.nordmann@example.com")]
    [InlineData("a@b.co", "a@b.co")]
    public void Parse_normalises_a_valid_address(string input, string expected)
    {
        // Act
        var result = EmailAddress.Parse(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("kari")]
    [InlineData("kari@")]
    [InlineData("@example.com")]
    [InlineData("kari@example")]
    [InlineData("kari@@example.com")]
    [InlineData("kari@.com")]
    [InlineData("kari@example.")]
    [InlineData("kari nordmann@example.com")]
    public void Parse_rejects_a_malformed_address(string input)
    {
        // Act
        var act = () => EmailAddress.Parse(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_rejects_an_address_over_the_cap()
    {
        // Arrange
        var input = new string('a', EmailAddress.MaxLength) + "@example.com";

        // Act
        var act = () => EmailAddress.Parse(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryParse_returns_the_normalised_address_when_valid()
    {
        // Act
        var ok = EmailAddress.TryParse(" Kari@Example.com ", out var result);

        // Assert
        ok.Should().BeTrue();
        result!.Value.Should().Be("kari@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kari@example")]
    public void TryParse_returns_false_without_throwing_when_malformed(string? input)
    {
        // Act
        var ok = EmailAddress.TryParse(input, out var result);

        // Assert
        ok.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void Equality_is_by_value()
    {
        // Arrange
        var left = EmailAddress.Parse("kari@example.com");
        var right = EmailAddress.Parse("KARI@example.com");

        // Act
        var equal = left == right;

        // Assert
        equal.Should().BeTrue();
    }
}
