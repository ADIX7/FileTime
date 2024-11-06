using Xunit;

namespace Signal.Tests;

public class DebounceTests
{
    [Fact]
    public async Task Debounce_WhenCalled_ReturnsDebouncedValue()
    {
        // Arrange
        var signal = new Signal<int>(1);
        var debouncedSignal = signal.Debounce(TimeSpan.FromMilliseconds(100));

        // Act
        var result1 = await debouncedSignal.GetValueAsync();
        await signal.SetValueAsync(2);
        var result2 = await debouncedSignal.GetValueAsync();
        await signal.SetValueAsync(3);
        var result3_1 = await debouncedSignal.GetValueAsync();

        await Task.Delay(300);
        var result3_2 = await debouncedSignal.GetValueAsync();
        await signal.SetValueAsync(4);
        var result4 = await debouncedSignal.GetValueAsync();

        // Assert
        Assert.Equal(1, result1);
        Assert.Equal(1, result2);
        Assert.Equal(1, result3_1);
        Assert.Equal(3, result3_2);
        Assert.Equal(3, result4);
    }
}