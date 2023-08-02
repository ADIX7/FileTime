using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.Core.Models;
using FileTime.Core.Models.ContainerTraits;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeScanContainer : ISizeItem, IContainer, IStatusProviderContainer
{
    public Task AddSizeSourceAsync(IDeclarativeProperty<long> sizeElement);
    ObservableCollection<ISizeScanContainer> ChildContainers { get; }
    ObservableCollection<ISizeScanElement> ChildElements { get; }
    ObservableCollection<ISizeItem> SizeItems { get; }
    IContainer RealContainer { get; init; }
    Task StartLoadingAsync();
    Task StopLoadingAsync();
}