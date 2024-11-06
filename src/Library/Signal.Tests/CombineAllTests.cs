using Xunit;

namespace Signal.Tests;

public class CombineAllTests
{
    [Fact]
    public async Task GetValueAsync_WhenParentSometimesReturnsNull_ShouldInitialize()
    {
        int count = 0;
        // Arrange
        var signal1 = new Signal<string>(null);
        var signal2 = new Signal<string>(null);
        var combineAll = new CombineAllSignal<string, string>([signal1, signal2], _ => "TEST");

        // Act
        var result = await combineAll.GetValueAsync();

        // Assert
        Assert.Equal("TEST", result);
    }
}