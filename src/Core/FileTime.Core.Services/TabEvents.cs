using FileTime.Core.Models;

namespace FileTime.Core.Services;

public class TabEvents : ITabEvents
{
    public event EventHandler<TabLocationChanged>? LocationChanged;
    
    public void OnLocationChanged(ITab tab, IContainer location) 
        => LocationChanged?.Invoke(this, new TabLocationChanged(location, tab));
}