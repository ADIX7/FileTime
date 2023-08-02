using DeclarativeProperty;

namespace FileTime.App.Core.Models.Traits;

public interface ISizeProvider
{
    IDeclarativeProperty<long> Size { get; }
}