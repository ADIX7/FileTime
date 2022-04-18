namespace FileTime.Core.Models
{
    public readonly struct ItemInitializationSettings
    {
        public readonly bool SkipChildInitialization;

        public ItemInitializationSettings(bool skipChildInitialization)
        {
            SkipChildInitialization = skipChildInitialization;
        }
    }
}