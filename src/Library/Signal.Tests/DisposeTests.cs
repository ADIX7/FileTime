using Xunit;

namespace Signal.Tests;

public class DisposeTests
{
    [Fact]
    public void Disposed_AfterDispose_ShouldBeTrue()
    {
        // Arrange
        var signal = new Signal<int>(0);
        
        // Act
        signal.Dispose();
        
        // Assert
        Assert.True(signal.IsDisposed);
    }
    
    [Fact]
    public void Disposed_AfterDispose_ShouldInvokeDisposedEvent()
    {
        // Arrange
        var signal = new Signal<int>(0);
        var disposedInvoked = false;
        signal.Disposed += _ => disposedInvoked = true;
        
        // Act
        signal.Dispose();
        
        // Assert
        Assert.True(disposedInvoked);
    }
    
    [Fact]
    public void ChildSignalDisposed_AfterParentSignalDispose_ShouldBeTrue()
    {
        // Arrange
        var parentSignal = new Signal<int>(0);
        var childSignal = parentSignal.Map(v => v);
        
        // Act
        parentSignal.Dispose();
        
        // Assert
        Assert.True(childSignal.IsDisposed);
    }
    
    [Fact]
    public void ChildSignalDisposed_AfterParentSignalDispose_ShouldInvokeDisposedEvent()
    {
        // Arrange
        var parentSignal = new Signal<int>(0);
        var childSignal = parentSignal.Map(v => v);
        var disposedInvoked = false;
        childSignal.Disposed += _ => disposedInvoked = true;
        
        // Act
        parentSignal.Dispose();
        
        // Assert
        Assert.True(disposedInvoked);
    }
}