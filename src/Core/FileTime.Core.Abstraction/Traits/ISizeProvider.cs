using DeclarativeProperty;

namespace FileTime.Core.Traits;

public interface ISizeProvider
{
    IDeclarativeProperty<long> Size { get; }
}