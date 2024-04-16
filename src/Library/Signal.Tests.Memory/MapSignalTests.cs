using JetBrains.dotMemoryUnit;

namespace Signal.Tests.Memory;

public class MapSignalTests
{
    [Fact]
    [AssertTraffic]
    public async Task Map_WhenNoAllocation_ShouldNoAllocationHappen()
    {
        // Arrange
        var signal = new Signal<int>(10);
        var mapped = signal.Map(s => s);
        var memorySnapShot = dotMemory.Check();
        
        // Act
        await mapped.GetValueAsync();
        
        // Assert
        //Assert.Equal(10, result);
        dotMemory.Check(memory =>
        {
            Assert.Equal(0, memory.GetDifference(memorySnapShot).GetNewObjects().ObjectsCount);
        });
    }
}