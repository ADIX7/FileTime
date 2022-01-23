using FileTime.Core.Models;

namespace FileTime.Core.Extensions
{
    public static class ContainerExtensions
    {
        public static async Task<IContainer> ToggleVirtualContainerInChain(this IContainer container, string filterName, Func<IContainer, Task<VirtualContainer>> generator)
        {
            if (container is VirtualContainer oldVirtualContainer)
            {
                return oldVirtualContainer.HasWithName(filterName)
                ? await oldVirtualContainer.ExceptWithName(filterName)
                : await generator(container);
            }
            else
            {
                return await generator(container);
            }
        }
        public static async Task<IContainer> WithoutVirtualContainer(this IContainer container, string filterName) =>
            container is VirtualContainer oldVirtualContainer
            ? await oldVirtualContainer.ExceptWithName(filterName)
            : container;
    }
}