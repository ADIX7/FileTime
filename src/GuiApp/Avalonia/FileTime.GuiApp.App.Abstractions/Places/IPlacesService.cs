using FileTime.App.Core.Services;
using FileTime.GuiApp.App.IconProviders;

namespace FileTime.GuiApp.App.Places;

public interface IPlacesService : IStartupHandler
{
    Dictionary<string, SpecialPathType> GetSpecialPaths();
}