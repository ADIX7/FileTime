using FileTime.Core.Models;
using FileTime.GuiApp.App.Services;

namespace FileTime.GuiApp.App.ContextMenu;

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