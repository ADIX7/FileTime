using Xunit;
using static Signal.Helpers;

namespace Signal.Tests;

public class CombineLatestTests
{
    [Fact]
    public async Task CombineLatest_With2ValidInput_ShouldCombineInputs()
    {
        // Arrange
        var signal1 = new Signal<string>("test1");
        var signal2 = new Signal<string>("test2");
        var combinedSignal = CombineLatest(signal1, signal2, (t1, t2) => t1 + " " + t2);

        // Act
        var result = await combinedSignal.GetValueAsync();
        
        // Assert
        Assert.Equal("test1 test2", result);
    }
    
    [Fact]
    public async Task CombineLatest_With2ValidInputAndAsyncCombiner_ShouldCombineInputs()
    {
        // Arrange
        var signal1 = new Signal<string>("test1");
        var signal2 = new Signal<string>("test2");
        var combinedSignal = CombineLatest(signal1, signal2, async (t1, t2) =>
        {
            await Task.Yield();
            return t1 + " " + t2;
        });

        // Act
        var result = await combinedSignal.GetValueAsync();
        
        // Assert
        Assert.Equal("test1 test2", result);
    }
}