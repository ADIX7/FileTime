using FileTime.Core.Models;

namespace FileTime.GuiApp.App.Services;

public class LinuxContextMenuProvider : IContextMenuProvider
{
    public List<object> GetContextMenuForFolder(IContainer container)
    {
        return new List<object>();
    }

    public List<object> GetContextMenuForFile(IElement element)
    {
        return new List<object>();
    }
}