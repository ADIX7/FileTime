using FileTime.App.Core.Models;

namespace FileTime.App.Core.Services;

public interface IAppKeyService<TKey>
{
    Keys? MapKey(TKey key);
}