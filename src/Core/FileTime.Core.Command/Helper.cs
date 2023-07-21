using FileTime.Core.Models;

namespace FileTime.Core.Command;

public static class Helper
{
    public static async Task<string?> GetNewNameAsync(IContainer resolvedTarget, string name, TransportMode transportMode)
    {
        await resolvedTarget.WaitForLoaded();
        var items = resolvedTarget.Items.ToList();
        var newName = name;
        var targetNameExists = items.Any(i => i.Path.GetName() == newName);
        if (transportMode == TransportMode.Merge)
        {
            for (var i = 0; targetNameExists; i++)
            {
                newName = name + (i == 0 ? "_" : $"_{i}");
                targetNameExists = resolvedTarget != null && items.Any(i => i.Path.GetName() == newName);
            }
        }
        else if (transportMode == TransportMode.Skip && targetNameExists)
        {
            return null;
        }

        return newName;
    }
}