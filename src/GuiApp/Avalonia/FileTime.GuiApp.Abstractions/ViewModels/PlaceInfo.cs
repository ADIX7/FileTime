using FileTime.Core.Models;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.ViewModels;

public class PlaceInfo : IHaveFullPath
{
    public IContainer Container { get; }
    public string DisplayName { get; }

    public PlaceInfo(IContainer container, string displayName)
    {
        if (container.FullName is null) throw new ArgumentNullException($"{nameof(container.FullName)} of container can not be null");

        Container = container;
        DisplayName = displayName;
    }

    public FullName Path => Container.FullName!;
}