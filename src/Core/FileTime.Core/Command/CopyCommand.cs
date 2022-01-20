using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CopyCommand : ITransportationCommand
    {
        public IList<IAbsolutePath> Sources { get; } = new List<IAbsolutePath>();

        public IContainer? Target { get; set; }

        public TransportMode TransportMode { get; set; } = TransportMode.Merge;

        public PointInTime SimulateCommand(PointInTime delta)
        {
            throw new NotImplementedException();
        }

        public async Task Execute(Action<IAbsolutePath, IAbsolutePath> copy)
        {
            await DoCopy(Sources, Target, TransportMode, copy);
        }

        private async Task DoCopy(IEnumerable<IAbsolutePath> sources, IContainer target, TransportMode transportMode, Action<IAbsolutePath, IAbsolutePath> copy)
        {
            foreach (var source in sources)
            {
                var item = await source.ContentProvider.GetByPath(source.Path);

                if (item is IContainer container)
                {
                    var targetContainer = (await target.GetContainers())?.FirstOrDefault(d => d.Name == container.Name) ?? (await target.CreateContainer(container.Name)!);

                    var childDirectories = (await container.GetContainers())!.Select(d => new AbsolutePath(item.Provider, d.FullName!));
                    var childFiles = (await container.GetElements())!.Select(f => new AbsolutePath(item.Provider, f.FullName!));

                    await DoCopy(childDirectories.Concat(childFiles), targetContainer, transportMode, copy);
                }
                else if (item is IElement element)
                {
                    var targetName = element.Name;

                    var targetNameExists = await target.IsExists(targetName);
                    if (transportMode == TransportMode.Merge)
                    {
                        for (var i = 0; targetNameExists; i++)
                        {
                            targetName = element.Name + (i == 0 ? "_" : $"_{i}");
                        }
                    }
                    else if (transportMode == TransportMode.Skip && targetNameExists)
                    {
                        continue;
                    }

                    var targetPath = target.FullName + Constants.SeparatorChar + targetName;

                    copy(new AbsolutePath(source.ContentProvider, element.FullName!), new AbsolutePath(target.Provider, targetPath));
                }
            }
        }
    }
}