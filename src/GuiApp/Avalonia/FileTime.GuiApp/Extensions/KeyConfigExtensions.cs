using FileTime.GuiApp.Configuration;

namespace FileTime.GuiApp.Extensions
{
    public static class KeyConfigExtensions
    {
        public static bool AreKeysEqual(this IReadOnlyList<KeyConfig> collection1, IReadOnlyList<KeyConfig> collection2)
        => collection1.Count == collection2.Count && collection1.Zip(collection2).All(t => t.First.AreEquals(t.Second));
    }
}