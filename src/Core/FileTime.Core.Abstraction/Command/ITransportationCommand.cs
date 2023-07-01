using FileTime.Core.Models;

namespace FileTime.Core.Command;

public interface ITransportationCommand : ICommand
{
    TransportMode TransportMode { get; }
    IReadOnlyList<FullName> Sources { get; }
    FullName Target { get; }
}