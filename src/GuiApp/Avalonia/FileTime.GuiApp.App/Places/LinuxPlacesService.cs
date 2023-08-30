using System.Collections;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.App.IconProviders;
using FileTime.GuiApp.App.ViewModels;
using FileTime.Providers.Local;

namespace FileTime.GuiApp.App.Places;

public class LinuxPlacesService : IPlacesService
{
    private readonly ILocalContentProvider _localContentProvider;
    private readonly IGuiAppState _guiAppState;
    private readonly Dictionary<string, SpecialPathType> _specialPath = new();

    public LinuxPlacesService(
        ILocalContentProvider localContentProvider,
        IGuiAppState guiAppState)
    {
        _localContentProvider = localContentProvider;
        _guiAppState = guiAppState;
    }

    public async Task InitAsync()
    {
        var placeKeys = new Dictionary<string, SpecialPathType>
        {
            {"XDG_DESKTOP_DIR", SpecialPathType.Desktop},
            {"XDG_DOWNLOAD_DIR", SpecialPathType.Downloads},
            {"XDG_TEMPLATES_DIR", SpecialPathType.Templates},
            {"XDG_PUBLICSHARE_DIR", SpecialPathType.PublicShare},
            {"XDG_DOCUMENTS_DIR", SpecialPathType.Documents},
            {"XDG_MUSIC_DIR", SpecialPathType.Music},
            {"XDG_PICTURES_DIR", SpecialPathType.Images},
            {"XDG_VIDEOS_DIR", SpecialPathType.Videos},
        };

        var homeFolder = Environment.GetEnvironmentVariable("HOME");
        if (homeFolder is null) return;

        var userDirsLines = await File.ReadAllLinesAsync(Path.Combine(homeFolder, ".config", "user-dirs.dirs"));

        var placesStrings = placeKeys
            .Select(p => (SpecialPath: p.Value, Line: userDirsLines.FirstOrDefault(l => l.StartsWith(p.Key))))
            .Where(l => l.Line is not null)
            .Select(l => (l.SpecialPath, Path: l.Line![(l.Line.IndexOf('=') + 1)..].Trim('\"')))
            .Select(l => (l.SpecialPath, Path: ReplaceEnvVars(l.Path)))
            .Where(l => Directory.Exists(l.Path))
            .ToList();

        var places = new List<PlaceInfo>();
        foreach (var place in placesStrings)
        {
            var resolvedPlace = await _localContentProvider.GetItemByNativePathAsync(new NativePath(place.Path), PointInTime.Present);

            if (resolvedPlace is not IContainer resolvedContainer) continue;
            places.Add(new PlaceInfo(resolvedContainer, resolvedContainer.DisplayName));
            _specialPath.Add(place.Path, place.SpecialPath);
        }

        var resolvedHomeItem = await _localContentProvider.GetItemByNativePathAsync(new NativePath(homeFolder), PointInTime.Present);


        IEnumerable<PlaceInfo> finalPlaces = places.OrderBy(p => p.DisplayName);

        if (resolvedHomeItem is IContainer resolvedHomeFolder)
        {
            finalPlaces = finalPlaces.Prepend(new PlaceInfo(resolvedHomeFolder, resolvedHomeFolder.DisplayName));
            _specialPath.Add(homeFolder, SpecialPathType.Home);
        }

        _guiAppState.Places = finalPlaces.ToList().AsReadOnly();
    }

    private string ReplaceEnvVars(string s)
    {
        return Environment
            .GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .Select(d => new KeyValuePair<string, string>(d.Key.ToString()!, d.Value!.ToString()!))
            .Aggregate(s, (c, kvp) => c.Replace("$" + kvp.Key, kvp.Value));
    }

    public Dictionary<string, SpecialPathType> GetSpecialPaths() => _specialPath;
}