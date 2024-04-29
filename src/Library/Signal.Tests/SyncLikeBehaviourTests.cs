using Xunit;

namespace Signal.Tests;

public class SyncLikeBehaviourTests
{
    [Fact(Timeout = 500)]
    public async Task Signal_WhenAwaitedInstantly_ShouldBehaveLikeSync()
    {
        // Arrange
        var signal = new Signal<int>(1);
        
        // Act
        var val1 = await signal.GetValueAsync();
        signal.SetValue(2);
        var val2 = await signal.GetValueAsync();
        signal.SetValue(3);
        var val3 = await signal.GetValueAsync();
        
        // Assert
        Assert.Equal(1, val1);
        Assert.Equal(2, val2);
        Assert.Equal(3, val3);
    }
    
    [Fact(Timeout = 500)]
    public async Task Signal_WhenNotAwaitedInstantly_ShouldBehaveLikeSync()
    {
        // Arrange
        var signal = new Signal<int>(1);
        
        // Act
        var val1 = signal.GetValueAsync();
        signal.SetValue(2);
        var val2 = signal.GetValueAsync();
        signal.SetValue(3);
        var val3 = signal.GetValueAsync();
        
        // Assert
        Assert.Equal(1, await val1);
        Assert.Equal(2, await val2);
        Assert.Equal(3, await val3);
    }
}