using FileTime.Core.Models;

namespace FileTime.Avalonia.Models
{
    public class PlaceInfo
    {
        public string Name { get; }
        public IContainer Container { get; }

        public PlaceInfo(string name, IContainer container)
        {
            Name = name;
            Container = container;
        }
    }
}