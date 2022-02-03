using System.Collections.Generic;

namespace FileTime.Avalonia.Models.Persistence
{
    public class TabStates
    {
        public List<TabState>? Tabs { get; set; }
        public int? ActiveTabNumber { get; set; }
    }
}