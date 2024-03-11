using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App.Services;

public interface IIconProvider
{
    string GetImage(IItem? item);
}