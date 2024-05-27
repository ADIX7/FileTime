using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Command.Copy;

public class CopyCommandFactory : ITransportationCommandFactory<CopyCommand>
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    private readonly CopyStrategyFactory _copyStrategyFactory;
    private readonly IServiceProvider _serviceProvider;

    public CopyCommandFactory(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        CopyStrategyFactory copyStrategyFactory,
        IServiceProvider serviceProvider)

    {
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        _copyStrategyFactory = copyStrategyFactory;
        _serviceProvider = serviceProvider;
    }

    public CopyCommand GenerateCommand(
        IReadOnlyCollection<FullName> sources,
        TransportMode mode,
        FullName targetFullName)
        => new(
            _timelessContentProvider,
            _commandSchedulerNotifier,
            _copyStrategyFactory,
            _serviceProvider.GetRequiredService<ILogger<CopyCommand>>(),
            sources,
            mode,
            targetFullName
        );
}