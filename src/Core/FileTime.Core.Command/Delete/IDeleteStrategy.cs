using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Core.Command.Delete;

public interface IDeleteStrategy
{
    Task DeleteItem(IItem item, IItemDeleter deleter);
}