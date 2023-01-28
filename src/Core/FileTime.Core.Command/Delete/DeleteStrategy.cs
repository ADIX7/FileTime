using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Core.Command.Delete;

public class DeleteStrategy : IDeleteStrategy
{
    public async Task DeleteItem(IItem item, IItemDeleter deleter)
    {
        await deleter.DeleteAsync(item.Provider, item.FullName!);
    }
}