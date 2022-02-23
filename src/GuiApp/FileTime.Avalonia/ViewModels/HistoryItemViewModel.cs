using FileTime.Core.Models;
using InitableService;
using System;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels
{
    public class HistoryItemViewModel : IAsyncInitable<AbsolutePath>
    {
        public string? Name { get; private set; }
        public IContainer? Container { get; private set; }

        public async Task InitAsync(AbsolutePath path)
        {
            Container = await path.ResolveAsync() as IContainer ?? throw new ArgumentException($"Parameter must be {nameof(IContainer)}", nameof(path));
            Name = Container.DisplayName;
        }
    }
}
