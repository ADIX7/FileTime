using FileTime.Core.Models;

namespace FileTime.Core.Command
{
    public interface ITransportationCommand : ICommand
    {
        IList<AbsolutePath>? Sources { get; }
        IContainer? Target { get; set;}
        TransportMode? TransportMode { get; set; }
    }
}