using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App.Services;

public partial class NerdFontIconProvider : IIconProvider
{
    private const string _fileIcon = "\uf15b"; //"\uea7b"

    public string GetImage(IItem? item)
        => item switch
        {
            IContainer container => GetContainerImage(container),
            IElement element => GetElementImage(element),
            _ => _fileIcon
        };
    private string GetContainerImage(IContainer container)
    {
        if (_directoryIcons.TryGetValue(container.Name, out var icon))
            return icon;
        return "\uf07b";
    }

    private string GetElementImage(IElement element)
    {
        if (_fileNameIcons.TryGetValue(element.Name, out var icon))
            return icon;
        if (_fileExtensionIcons.TryGetValue(element.Name.Split('.').LastOrDefault() ?? "", out icon))
            return icon;
        return _fileIcon;
    }
}