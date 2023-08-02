using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public interface IContainerSizeScanContainer : ISizeItem, IContainer
{
    public Task AddSizeSourceAsync(IDeclarativeProperty<long> sizeElement);
    ObservableCollection<IContainerSizeScanContainer> ChildContainers { get; }
    IContainer RealContainer { get; init; }
    IDeclarativeProperty<long> Size { get; }
}