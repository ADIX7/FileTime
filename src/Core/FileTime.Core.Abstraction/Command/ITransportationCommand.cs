using FileTime.Core.Models;

namespace FileTime.Core.Command;

public interface ITransportationCommand : ICommand
{
    TransportMode? TransportMode { get; set; }
    IList<FullName> Sources { get; }
    FullName? Target { get; set; }
}