using Xunit;

namespace Signal.Tests;

public class LockingTests
{
    // These tests are not working, but you get the idea, figure them out sometimes in the future, gl&hf
    
    [Fact]
    public async Task SetAndGet_WhenGetRunsFirst_ShouldNotDeadlock()
    {
        // Arrange
        var signal = new Signal<int>(0);
        var childSignal = signal.Map(async v =>
        {
            await Task.Delay(200);
            return v;
        });
        
        // Act
        await Task.WhenAll(
            Task.Run(async () => await signal.GetValueAsync()),
            Task.Run(() => signal.SetValue(1))
        );
        
        // Assert
        // If this does not deadlock we are okay
    }
    
    [Fact]
    public async Task SetAndGet_WhenSetRunsFirst_ShouldNotDeadlock()
    {
        // Arrange
        var signal = new Signal<int>(0);
        var childSignal = signal.Map(async v =>
        {
            await Task.Delay(200);
            return v;
        });
        
        // Act
        await Task.WhenAll(
            Task.Run(() => signal.SetValue(1)),
            Task.Run(async () => await signal.GetValueAsync())
        );
        
        // Assert
        // If this does not deadlock we are okay
    }
}