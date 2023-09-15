using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.Tools.Extensions;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompressCompressionType = SharpCompress.Common.CompressionType;

namespace FileTime.Tools.Compression.Compress;

public class CompressCommand : CommandBase, IExecutableCommand, ITransportationCommand, IRequireInputCommand
{
    private readonly IList<IInputElement> _inputs;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    private readonly OptionsInputElement<CompressionType> _compressionType;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TextInputElement _targetFileName;
    public IReadOnlyList<FullName> Sources { get; }
    public FullName Target { get; }
    public TransportMode TransportMode { get; }


    internal CompressCommand(
        IUserCommunicationService userCommunicationService,
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        IReadOnlyCollection<FullName> sources,
        TransportMode mode,
        FullName targetFullName)
    {
        _userCommunicationService = userCommunicationService;
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(targetFullName);

        Sources = new List<FullName>(sources).AsReadOnly();
        TransportMode = mode;
        Target = targetFullName;

        _targetFileName = new TextInputElement("File name");
        _compressionType = new OptionsInputElement<CompressionType>(
            "CompressionMethod",
            Enum.GetValues<CompressionType>()
                .Select(t => new OptionElement<CompressionType>(t.ToString(), t))
        )
        {
            Value = CompressionType.Zip
        };

        _inputs = new List<IInputElement>
        {
            _targetFileName,
            _compressionType
        };
    }


    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(CanCommandRun.True);
    }

    public override Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        var differences = new List<Difference>
        {
            new(DifferenceActionType.Create, new AbsolutePath(_timelessContentProvider, currentTime, Target, AbsolutePathType.Element))
        };
        return Task.FromResult(currentTime.WithDifferences(differences));
    }

    public override void Cancel()
        => _cancellationTokenSource.Cancel();

    public async Task Execute()
    {
        var disposables = Enumerable.Empty<IDisposable>();

        ICompressOperation? compressOperation = null;
        try
        {
            var compressionType = _compressionType.Value;

            compressOperation = compressionType switch
            {
                CompressionType.Gzip => SharpCompress.Archives.GZip.GZipArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(SharpCompressCompressionType.GZip)))),
                CompressionType.Zip => SharpCompress.Archives.Zip.ZipArchive.Create().Map(a => GetCompressOperation(a, a.SaveTo)),
                CompressionType.Tar => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(SharpCompressCompressionType.None)))),
                CompressionType.TarBz2 => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(SharpCompressCompressionType.BZip2)))),
                CompressionType.TarLz => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(SharpCompressCompressionType.LZip)))),
                _ => throw new NotImplementedException()
            };

            disposables = await TraverseTree(Sources, "", compressOperation);

            var resolvedParent = await _timelessContentProvider.GetItemByFullNameAsync(
                Target,
                PointInTime.Present
            );

            var newItemName = Target.GetChild(_targetFileName.Value!);
            await _contentAccessorFactory.GetItemCreator(resolvedParent.Provider).CreateElementAsync(resolvedParent.Provider, newItemName);
            var targetElement = (IElement) await _timelessContentProvider.GetItemByFullNameAsync(newItemName, PointInTime.Present);
            using var contentWriter = await _contentAccessorFactory.GetContentWriterFactory(resolvedParent.Provider).CreateContentWriterAsync(targetElement);
            await using var contentWriterStream = contentWriter.GetStream();
            compressOperation.SaveTo(contentWriterStream);

            await contentWriterStream.FlushAsync(_cancellationTokenSource.Token);
        }
        finally
        {
            compressOperation?.Dispose();

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
        
        await _commandSchedulerNotifier.RefreshContainer(Target);
    }

    private async Task<IEnumerable<IDisposable>> TraverseTree(
        IEnumerable<FullName> sources,
        string basePath,
        ICompressOperation operations,
        CancellationToken cancellationToken = default)
    {
        var disposables = Enumerable.Empty<IDisposable>();
        foreach (var source in sources)
        {
            var item = await _timelessContentProvider.GetItemByFullNameAsync(
                source,
                PointInTime.Present
            );

            if (item is IContainer container)
            {
                var items = container.Items;

                var childItems = items.Select(i => i.Path).ToList();
                var path = string.IsNullOrEmpty(basePath) ? container.Name : basePath + "\\" + container.Name;
                disposables = disposables.Concat(await TraverseTree(childItems, path, operations, cancellationToken));
            }
            else if (item is IElement element)
            {
                var path = string.IsNullOrEmpty(basePath) ? element.Name : basePath + "\\" + element.Name;
                disposables = disposables.Concat(await operations.CompressElement(element, path));
            }
        }

        return disposables;
    }

    private CompressOperation<TEntry, TVolume> GetCompressOperation<TEntry, TVolume>(AbstractWritableArchive<TEntry, TVolume> archive, Action<Stream> saveTo)
        where TEntry : IArchiveEntry where TVolume : IVolume
        => new(_contentAccessorFactory, archive, saveTo);

    public async Task ReadInputs()
        => await _userCommunicationService.ReadInputs(_inputs);
}