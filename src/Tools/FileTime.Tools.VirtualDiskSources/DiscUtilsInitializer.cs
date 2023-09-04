using FileTime.App.Core.Services;

namespace FileTime.Tools.VirtualDiskSources;

public class DiscUtilsInitializer : IPreStartupHandler
{
    public Task InitAsync()
    {
        DiscUtils.Containers.SetupHelper.SetupContainers();
        DiscUtils.FileSystems.SetupHelper.SetupFileSystems();

        return Task.CompletedTask;
    }
}