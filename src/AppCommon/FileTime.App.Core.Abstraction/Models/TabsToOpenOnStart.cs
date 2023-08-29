using FileTime.Core.Models;

namespace FileTime.App.Core.Models;

public record TabToOpen(int? TabNumber, NativePath Path);

public record TabsToOpenOnStart(List<TabToOpen> TabsToOpen);