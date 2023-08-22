using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression;

public class DecompressCommand : CommandBase, IExecutableCommand, ITransportationCommand, IDisposable
{
    private record ArchiveContext(IArchive Archive, string TargetContainerName);

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    private readonly List<ArchiveContext> _archives = new();
    private readonly List<IDisposable> _disposables = new();

    public IReadOnlyList<FullName> Sources { get; }
    public FullName Target { get; }
    public TransportMode TransportMode { get; }

    internal DecompressCommand(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        IReadOnlyCollection<FullName> sources,
        TransportMode mode,
        FullName targetFullName)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(targetFullName);

        Sources = new List<FullName>(sources).AsReadOnly();
        TransportMode = mode;
        Target = targetFullName;
    }


    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(CanCommandRun.True);
    }

    public override Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(currentTime);
    }


    public override void Cancel()
        => _cancellationTokenSource.Cancel();

    public async Task Execute()
    {
        if (_archives.Count == 0) await OpenArchivesAsync();

        var resolvedTarget = (IContainer) await _timelessContentProvider.GetItemByFullNameAsync(Target, PointInTime.Present);
        var itemCreator = _contentAccessorFactory.GetItemCreator(resolvedTarget.Provider);
        var contentWriterFactory = _contentAccessorFactory.GetContentWriterFactory(resolvedTarget.Provider);

        foreach (var archiveContext in _archives)
        {
            await DecompressFile(archiveContext, resolvedTarget, contentWriterFactory, itemCreator);
        }
    }

    private async Task DecompressFile(
        ArchiveContext archiveContext,
        IContainer resolvedTarget,
        IContentWriterFactory contentWriterFactory,
        IItemCreator itemCreator)
    {
        var archive = archiveContext.Archive;
        foreach (var archiveEntry in archive.Entries)
        {
            var subPath = string.Join(Constants.SeparatorChar, archiveEntry.Key.Split('\\'));
            var entryPath = Target.GetChild(subPath);
            
            if (archiveEntry.IsDirectory)
            {
                await itemCreator.CreateContainerAsync(resolvedTarget.Provider, entryPath);
            }
            else
            {
                await itemCreator.CreateElementAsync(resolvedTarget.Provider, entryPath);
                var newItem = (IElement) await _timelessContentProvider.GetItemByFullNameAsync(entryPath, PointInTime.Present);
                using var writer = await contentWriterFactory.CreateContentWriterAsync(newItem);

                archiveEntry.WriteTo(writer.AsStream());
            }
        }
    }

    private async Task OpenArchivesAsync()
    {
        if (_archives.Count > 0) throw new InvalidOperationException("Archives are already open");
        foreach (var source in Sources)
        {
            var targetElement = (IElement) await _timelessContentProvider.GetItemByFullNameAsync(source, PointInTime.Present);
            var contentReader = await _contentAccessorFactory.GetContentReaderFactory(targetElement.Provider).CreateContentReaderAsync(targetElement);
            var contentReaderStream = contentReader.AsStream();
            _disposables.Add(contentReader);
            using var archive = ArchiveFactory.Open(contentReaderStream);

            _archives.Add(new ArchiveContext(archive, GetFileName(source.GetName())));
        }
    }

    public string GetFileName(string fullName)
    {
        var parts = fullName.Split('.');
        var fileName = string.Join('.', parts[..^1]);
        return string.IsNullOrEmpty(fileName) ? fullName : fileName;
    }

    public void Dispose()
    {
        foreach (var archive in _archives)
        {
            archive.Archive.Dispose();
        }
    }
}