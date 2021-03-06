using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface IClipboardService
{
    Type? CommandType { get; }
    IReadOnlyList<FullName> Content { get; }

    void AddContent(FullName absolutePath);
    void RemoveContent(FullName absolutePath);
    void Clear();
    void SetCommand<T>() where T : ITransportationCommand;
}