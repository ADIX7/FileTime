using FileTime.Core.Models;

namespace FileTime.GuiApp.App.Services;

public interface IContextMenuProvider
{
    List<object> GetContextMenuForFolder(IContainer container);
    List<object> GetContextMenuForFile(IElement element);
}