using FileTime.App.Core.Services;
using FileTime.GuiApp.IconProviders;

namespace FileTime.GuiApp.Services;

public interface IPlacesService : IStartupHandler
{
    Dictionary<string, SpecialPathType> GetSpecialPaths();
}