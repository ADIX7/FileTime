using FileTime.Core.Models;

namespace FileTime.GuiApp.Services;

public interface IContextMenuProvider
{
    List<object> GetContextMenuForFolder(IContainer container);
}