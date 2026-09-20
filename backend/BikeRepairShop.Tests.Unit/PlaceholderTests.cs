using AwesomeAssertions;

namespace BikeRepairShop.Tests.Unit;

public class PlaceholderTests
{
    [Fact]
    public void Runner_executes_a_test()
    {
        // Arrange
        var value = 1;

        // Act
        var result = value + 1;

        // Assert
        result.Should().Be(2);
    }
}
