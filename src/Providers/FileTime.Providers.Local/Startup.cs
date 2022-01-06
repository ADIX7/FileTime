using FileTime.Providers.Local.CommandHandlers;

namespace FileTime.Providers.Local
{
    public static class Startup
    {
        public static Type[] GetCommandHandlers()
        {
            return new Type[]{
                typeof(CopyCommandHandler)
            };
        }
    }
}