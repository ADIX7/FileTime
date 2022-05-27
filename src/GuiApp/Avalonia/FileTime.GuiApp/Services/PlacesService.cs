using System.Runtime.InteropServices;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.ViewModels;
using FileTime.Providers.Local;
using Syroot.Windows.IO;

namespace FileTime.GuiApp.Services;

public class PlacesService : IStartupHandler
{
    private readonly ILocalContentProvider _localContentProvider;
    private readonly IGuiAppState _guiAppState;

    public PlacesService(
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
}