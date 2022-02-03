using FileTime.Avalonia.Application;

namespace FileTime.Avalonia.Models.Persistence
{
    public class TabState
    {
        public string? Path { get; set; }
        public int Number { get; set; }

        public TabState() { }

        public TabState(TabContainer tab)
        {
            Path = tab.CurrentLocation.Item.FullName;
            Number = tab.TabNumber;
        }
    }
}