namespace FileTime.Core.Persistence
{
    public class PersistenceSettings
    {
        public PersistenceSettings(string rootAppDataPath)
        {
            RootAppDataPath = rootAppDataPath;
        }

        public string RootAppDataPath { get; }
    }
}