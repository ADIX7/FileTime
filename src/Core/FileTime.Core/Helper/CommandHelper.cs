using FileTime.Core.Models;

namespace FileTime.Core.Helper
{
    public static class CommandHelper
    {
        public static async Task<string?> GetNewNameAsync(IContainer? resolvedTarget, string name, Command.TransportMode transportMode)
        {
            var newName = name;
            var targetNameExists = resolvedTarget != null && await resolvedTarget.IsExistsAsync(newName);
            if (transportMode == Command.TransportMode.Merge && resolvedTarget != null)
            {
                for (var i = 0; targetNameExists; i++)
                {
                    newName = name + (i == 0 ? "_" : $"_{i}");
                    targetNameExists = await resolvedTarget.IsExistsAsync(newName);
                }
            }
            else if (transportMode == Command.TransportMode.Skip && targetNameExists)
            {
                return null;
            }

            return newName;
        }
    }
}