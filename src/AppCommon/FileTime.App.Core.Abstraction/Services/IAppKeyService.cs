using GeneralInputKey;

namespace FileTime.App.Core.Services;

public interface IAppKeyService<TKey>
{
    Keys? MapKey(TKey key);
}