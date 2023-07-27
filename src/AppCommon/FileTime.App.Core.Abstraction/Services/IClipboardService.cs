using System.Collections.ObjectModel;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface IClipboardService
{
    Type? CommandFactoryType { get; }
    ReadOnlyObservableCollection<FullName> Content { get; }

    void AddContent(FullName absolutePath);
    void RemoveContent(FullName absolutePath);
    void Clear();
    void SetCommand<T>() where T : ITransportationCommandFactory;
}