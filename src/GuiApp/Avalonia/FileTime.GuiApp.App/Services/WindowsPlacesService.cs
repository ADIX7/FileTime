using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.App.IconProviders;
using FileTime.GuiApp.App.ViewModels;
using FileTime.Providers.Local;
using Syroot.Windows.IO;

namespace FileTime.GuiApp.App.Services;

public class WindowsPlacesService : IPlacesService
{
    private readonly ILocalContentProvider _localContentProvider;
    private readonly IGuiAppState _guiAppState;

    public WindowsPlacesService(
        ILocalContentProvider localContentProvider,
        IGuiAppState guiAppState)
    {
        _localContentProvider = localContentProvider;
        _guiAppState = guiAppState;
    }

    public async Task InitAsync()
    {
        var places = new List<PlaceInfo>();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var placesFolders = new List<KnownFolder>()
            {
                KnownFolders.Profile,
                KnownFolders.Desktop,
                KnownFolders.DocumentsLocalized,
                KnownFolders.DownloadsLocalized,
                KnownFolders.Music,
                KnownFolders.Pictures,
                KnownFolders.Videos,
            };

            foreach (var placesFolder in placesFolders)
            {
                var possibleContainer = await _localContentProvider.GetItemByNativePathAsync(new NativePath(placesFolder.Path), PointInTime.Present);
                if (possibleContainer is not IContainer container) continue;


                places.Add(new PlaceInfo(container, placesFolder.DisplayName));
            }
        }

        _guiAppState.Places = places.AsReadOnly();
    }

    public Dictionary<string, SpecialPathType> GetSpecialPaths() 
        => new()
        {
            {KnownFolders.Desktop.Path, SpecialPathType.Desktop},
            {KnownFolders.Documents.Path, SpecialPathType.Documents},
            {KnownFolders.DownloadsLocalized.Path, SpecialPathType.Downloads},
            {KnownFolders.MusicLocalized.Path, SpecialPathType.Music},
            {KnownFolders.Pictures.Path, SpecialPathType.Images},
            {KnownFolders.Profile.Path, SpecialPathType.Home},
            {KnownFolders.Videos.Path, SpecialPathType.Videos},
        };
}