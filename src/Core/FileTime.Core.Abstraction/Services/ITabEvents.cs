using FileTime.Core.Models;

namespace FileTime.Core.Services;

public interface ITabEvents
{
    event EventHandler<TabLocationChanged> LocationChanged;
    void OnLocationChanged(ITab tab, IContainer location);
}