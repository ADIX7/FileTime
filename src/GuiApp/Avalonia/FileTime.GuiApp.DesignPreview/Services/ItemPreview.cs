using System.Collections.ObjectModel;
using DynamicData;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.GuiApp.DesignPreview.Services;

public class ItemPreview
{
    public static IContainer CurrentContainer { get; }

    static ItemPreview()
    {
        var exceptions = new ObservableCollection<Exception>();
        CurrentContainer = new Container(
            "HomePreview",
            "HomePreview",
            new FullName("local/root/test/path/HomePreview"),
            new NativePath("/root/test/path/HomePreview"),
            new AbsolutePath(
                null!,
                PointInTime.Present,
                new FullName("local/root/test/path"),
                AbsolutePathType.Container
            ),
            false,
            true,
            DateTime.Now,
            SupportsDelete.True,
            true,
            "attr",
            null!,
            false,
            PointInTime.Present,
            exceptions,
            new ExtensionCollection().AsReadOnly(),
            new ObservableCollection<AbsolutePath>()
        );
    }

    public static IElement GenerateElement(string name, string parentPath = "local/root/test/path/HomePreview") =>
        new Element(
            name,
            name,
            new FullName(parentPath + "/" + name),
            new NativePath("/root/test/path/HomePreview/" + name),
            new AbsolutePath(
                null!,
                PointInTime.Present,
                new FullName(parentPath),
                AbsolutePathType.Container
            ),
            false,
            true,
            DateTime.Now,
            SupportsDelete.True,
            true,
            "attr",
            null!,
            PointInTime.Present, 
            new ObservableCollection<Exception>(),
            new ExtensionCollection().AsReadOnly()
        );
}