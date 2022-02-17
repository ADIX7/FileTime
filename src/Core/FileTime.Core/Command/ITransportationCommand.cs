using FileTime.Core.Interactions;
using FileTime.Core.Models;

namespace FileTime.Core.Command
{
    public interface ITransportationCommand : ICommand
    {
        IList<AbsolutePath> Sources { get; }
        AbsolutePath? Target { get; set; }
        TransportMode? TransportMode { get; set; }
        bool TargetIsContainer { get; }
        List<InputElement> Inputs { get; }
        List<object>? InputResults { get; set; }
    }
}