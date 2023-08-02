using DeclarativeProperty;

namespace FileTime.Core.Models.ContainerTraits;

public interface IStatusProviderContainer
{
    IDeclarativeProperty<string> Status { get; }
}