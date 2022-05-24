namespace FileTime.App.Core.Models;

public interface IApplicationSettings
{
    string AppDataRoot { get; }
    string EnvironmentName { get; }
}