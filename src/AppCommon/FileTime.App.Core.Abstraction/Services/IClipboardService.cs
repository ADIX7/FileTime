using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface IClipboardService
{
    Type? CommandType { get; }
    IReadOnlyList<IAbsolutePath> Content { get; }

    void AddContent(IAbsolutePath absolutePath);
    void RemoveContent(IAbsolutePath absolutePath);
    void Clear();
    void SetCommand<T>() where T : ITransportationCommand;
}