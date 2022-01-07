using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class DeleteCommand : IExecutableCommand
    {
        public IList<IAbsolutePath> ItemsToDelete { get; } = new List<IAbsolutePath>();

        public PointInTime SimulateCommand(PointInTime delta)
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            foreach (var item in ItemsToDelete)
            {
                DoDelete(item.ContentProvider.GetByPath(item.Path)!);
            }
        }

        private void DoDelete(IItem item)
        {
            if (item is IContainer container)
            {
                foreach (var child in container.Items)
                {
                    DoDelete(child);
                    child.Delete();
                }

                item.Delete();
            }
            else if(item is IElement element)
            {
                element.Delete();
            }
        }
    }
}