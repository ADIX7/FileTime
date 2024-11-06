using Xunit;

namespace Signal.Tests;

public class MapSignalTests
{
    [Fact]
    public void Map_WhenNotRead_ShouldBeDirty()
    {
        // Arrange
        var signal = new Signal<string>("test");
        var mapped = signal.Map(s => s);
        
        // Act
        
        // Assert
        Assert.True(mapped.IsDirty);
    }
    
    [Fact]
    public async Task Map_WhenNotReadButBaseIsNotDirty_ShouldBeDirty()
    {
        // Arrange
        var signal = new Signal<string>("test");
        await signal.GetValueAsync();
        var mapped = signal.Map(s => s);
        
        // Act
        
        // Assert
        Assert.True(mapped.IsDirty);
    }
    
    [Fact]
    public async Task Map_WhenAlreadyRead_ShouldNotBeDirty()
    {
        // Arrange
        var signal = new Signal<string>("test");
        var mapped = signal.Map(s => s);
        
        // Act
        await mapped.GetValueAsync();
        
        // Assert
        Assert.False(mapped.IsDirty);
    }
    
    [Fact]
    public async Task GetValueAsync_WithSyncMapper_ShouldReturnCorrectData()
    {
        // Arrange
        var signal = new Signal<string>("tEsT");
        var mapped = signal.Map(s => s.ToUpper());
        
        // Act
        var result = await mapped.GetValueAsync();
        
        // Assert
        Assert.Equal("TEST", result);
    }
    
    [Fact]
    public async Task GetValueAsync_WithAsyncMapper_ShouldReturnCorrectData()
    {
        // Arrange
        var signal = new Signal<string>("tEsT");
        var mapped = signal.Map(async s =>
        {
            await Task.Yield();
            return s.ToUpper();
        });
        
        // Act
        var result = await mapped.GetValueAsync();
        
        // Assert
        Assert.Equal("TEST", result);
    }

    [Fact]
    public async Task GetValueAsync_WhenParentSometimesReturnsNull_ShouldInitialize()
    {
        int count = 0;
        // Arrange
        var signal = new Signal<string>(null);
        var mapped = signal.Map(_ => "TEST");
        
        // Act
        var result = await mapped.GetValueAsync();
        
        // Assert
        Assert.Equal("TEST", result);
    }
}