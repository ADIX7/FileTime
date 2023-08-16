using FileTime.Core.Models;
using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.ViewModels;

public class PlaceInfo
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