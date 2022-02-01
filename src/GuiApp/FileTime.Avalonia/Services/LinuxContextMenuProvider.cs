using System.Collections.Generic;
using FileTime.Core.Models;

namespace FileTime.Avalonia.Services
{
    public class LinuxContextMenuProvider : IContextMenuProvider
    {
        public List<object> GetContextMenuForFolder(IContainer container)
        {
            return new List<object>();
        }
    }
}