using FileTime.Core.Models;

namespace FileTime.Core.Command
{
    public interface ITransportationCommand : ICommand
    {
        IList<IAbsolutePath> Sources { get; }
        IContainer Target { get; set;}
        TransportMode TransportMode { get; set; }
    }
}