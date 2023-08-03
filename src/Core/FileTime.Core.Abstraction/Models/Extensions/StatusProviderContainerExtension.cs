using DeclarativeProperty;

namespace FileTime.Core.Models.Extensions;

public class StatusProviderContainerExtension
{
    public StatusProviderContainerExtension(Func<IDeclarativeProperty<string>> getStatusProperty)
    {
        GetStatusProperty = getStatusProperty;
    }

    public Func<IDeclarativeProperty<string>> GetStatusProperty { get; }
}