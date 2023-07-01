using FileTime.Core.Models;

namespace FileTime.Core.Command;

public interface ITransportationCommandFactory
{
    ITransportationCommand GenerateCommand(
        IReadOnlyCollection<FullName> sources, 
        TransportMode mode, 
        FullName targetFullName);
}

public interface ITransportationCommandFactory<out T> : ITransportationCommandFactory where T : ITransportationCommand
{
    new T GenerateCommand(IReadOnlyCollection<FullName> sources, 
        TransportMode mode, 
        FullName targetFullName);
    
    ITransportationCommand ITransportationCommandFactory.GenerateCommand(
        IReadOnlyCollection<FullName> sources, 
        TransportMode mode, 
        FullName targetFullName) 
        => GenerateCommand(sources, mode, targetFullName);
}