using FileTime.Core.Models;
using System.Collections.Generic;

namespace FileTime.Avalonia.Services
{
    public interface IContextMenuProvider
    {
        List<object> GetContextMenuForFolder(IContainer container);
    }
}