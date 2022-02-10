using FileTime.Avalonia.Application;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Avalonia.Models.Persistence
{
    public class TabState
    {
        public string? Path { get; set; }
        public int Number { get; set; }

        public TabState() { }

        public TabState(TabContainer tab)
        {
            var item = tab.CurrentLocation.Item;
            Path = item is IContentProvider contentProvider ? Constants.ContentProviderProtocol + contentProvider.Name : item.FullName;
            Number = tab.TabNumber;
        }
    }
}