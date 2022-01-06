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

        public void Execute(Action<IAbsolutePath, IAbsolutePath> copy)
        {
            DoCopy(Sources, Target, TransportMode, copy);
        }

        private void DoCopy(IEnumerable<IAbsolutePath> sources, IContainer target, TransportMode transportMode, Action<IAbsolutePath, IAbsolutePath> copy)
        {
            foreach (var source in sources)
            {
                var item = source.ContentProvider.GetByPath(source.Path);

                if (item is IContainer container)
                {
                    var targetContainer = target.Containers.FirstOrDefault(d => d.Name == container.Name) ?? (target.CreateContainer(container.Name)!);

                    var childDirectories = container.Containers.Select(d => new AbsolutePath(item.Provider, d.FullName!));
                    var childFiles = container.Elements.Select(f => new AbsolutePath(item.Provider, f.FullName!));

                    DoCopy(childDirectories.Concat(childFiles), targetContainer, transportMode, copy);
                }
                else if (item is IElement element)
                {
                    var targetName = element.Name;

                    if (transportMode == TransportMode.Merge)
                    {
                        for (var i = 0; target.IsExists(targetName); i++)
                        {
                            targetName = element.Name + (i == 0 ? "_" : $"_{i}");
                        }
                    }
                    else if (transportMode == TransportMode.Skip && target.IsExists(targetName))
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