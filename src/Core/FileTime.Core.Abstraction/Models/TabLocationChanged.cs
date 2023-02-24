using FileTime.Core.Services;

namespace FileTime.Core.Models;

public class TabLocationChanged : EventArgs
{
    public FullName Location { get; }
    public ITab Tab { get; }
    
    public TabLocationChanged(FullName location, ITab tab)
    {
        Location = location;
        Tab = tab;
    }
}