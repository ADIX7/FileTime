using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeScanContainer : ISizeItem, IContainer
{
    public Task AddSizeSourceAsync(IDeclarativeProperty<long> sizeElement);
    ObservableCollection<ISizeScanContainer> ChildContainers { get; }
    ObservableCollection<ISizeItem> SizeItems { get; }
    IContainer RealContainer { get; init; }
    Task StartLoadingAsync();
    Task StopLoadingAsync();
    Task AddSizeChildAsync(ISizeItem newChild);
}