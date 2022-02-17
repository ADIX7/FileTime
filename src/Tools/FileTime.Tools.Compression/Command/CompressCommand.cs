using AsyncEvent;
using FileTime.Core.Command;
using FileTime.Core.Extensions;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Core.Timeline;
using FileTime.Tools.Compression.Command.OperationHandlers;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileTime.Tools.Compression.Command
{
    public class CompressCommand : IExecutableCommand, ITransportationCommand
    {
        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();
        public AbsolutePath? Target { get; set; }
        public string DisplayLabel { get; } = "Compress";

        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public int Progress { get; }
        public int CurrentProgress { get; }

        public AsyncEventHandler ProgressChanged { get; } = new AsyncEventHandler();
        public TransportMode? TransportMode { get; set; }
        public bool TargetIsContainer => false;
        public List<InputElement> Inputs { get; }
        public List<object>? InputResults { get; set; }

        public CompressCommand()
        {
            Inputs = new List<InputElement>()
            {
                InputElement.ForOptions("Compression method", Enum.GetValues<Models.CompressionType>().Cast<object>())
            };
        }

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            //TODO: implement
            return Task.FromResult(CanCommandRun.True);
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            //TODO: implement
            return Task.FromResult(startPoint.WithDifferences(new List<Difference>()));
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");

            var disposables = Enumerable.Empty<IDisposable>();

            ICompressOperation? compressOperation = null;
            try
            {
                var compressionType =
                    InputResults?.Count > 0 && InputResults[0] is Models.CompressionType compType
                    ? compType
                    : Models.CompressionType.Zip;

                compressOperation = compressionType switch
                {
                    Models.CompressionType.Gzip => SharpCompress.Archives.GZip.GZipArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(CompressionType.GZip)))),
                    Models.CompressionType.Zip => SharpCompress.Archives.Zip.ZipArchive.Create().Map(a => GetCompressOperation(a, a.SaveTo)),
                    Models.CompressionType.Tar => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(CompressionType.None)))),
                    Models.CompressionType.TarBz2 => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(CompressionType.BZip2)))),
                    Models.CompressionType.TarLz => SharpCompress.Archives.Tar.TarArchive.Create().Map(a => GetCompressOperation(a, (s) => a.SaveTo(s, new SharpCompress.Writers.WriterOptions(CompressionType.LZip)))),
                    _ => throw new NotImplementedException()
                };

                disposables = await TraverseTree(Sources, "", compressOperation);

                var resolvedParent = (IContainer)(await Target.GetParent().ResolveAsync())!;
                var targetElement = await resolvedParent.CreateElementAsync(Target.GetName());

                using var contentWriter = await targetElement.GetContentWriterAsync();
                using var contentWriterStream = new ContentProviderStream(contentWriter);
                compressOperation.SaveTo(contentWriterStream);

                await contentWriterStream.FlushAsync();
            }
            finally
            {
                compressOperation?.Dispose();

                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            }
        }

        private static async Task<IEnumerable<IDisposable>> TraverseTree(
            IEnumerable<AbsolutePath> sources,
            string basePath,
            ICompressOperation operations)
        {
            var disposables = Enumerable.Empty<IDisposable>();
            foreach (var source in sources)
            {
                var item = await source.ResolveAsync();

                if (item is IContainer container)
                {
                    var items = await container.GetItems();
                    if (items == null) continue;

                    var childItems = items.Select(i => new AbsolutePath(i)).ToList()!;
                    var path = string.IsNullOrEmpty(basePath) ? container.Name : basePath + "\\" + container.Name;
                    disposables = disposables.Concat(await TraverseTree(childItems, path, operations));
                }
                else if (item is IElement element)
                {
                    var path = string.IsNullOrEmpty(basePath) ? element.Name : basePath + "\\" + element.Name;
                    disposables = disposables.Concat(await operations.CompressElement(element, path));
                }
            }
            return disposables;
        }

        public static CompressOperation<TEntry, TVolume> GetCompressOperation<TEntry, TVolume>(AbstractWritableArchive<TEntry, TVolume> archive, Action<Stream> saveTo)
            where TEntry : IArchiveEntry
            where TVolume : IVolume
        {
            return new CompressOperation<TEntry, TVolume>(archive, saveTo);
        }
    }
}