namespace FileTime.App.Database;

public interface IDatabaseContext
{
    ValueTask<IDatabaseConnection> GetConnectionAsync();
}