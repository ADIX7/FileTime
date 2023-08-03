using FileTime.Core.Services;

namespace FileTime.Core.Models;

public class TabLocationChanged : EventArgs
{
    public IContainer Location { get; }
    public ITab Tab { get; }
    
    public TabLocationChanged(IContainer location, ITab tab)
    {
        Location = location;
        Tab = tab;
    }
}