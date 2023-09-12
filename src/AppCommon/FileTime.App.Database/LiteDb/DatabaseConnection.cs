using FileTime.App.Core.Models;
using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class DatabaseConnection : IDatabaseConnection
{
    private readonly ILiteDatabase _liteDb;

    public DatabaseConnection(IApplicationSettings applicationSettings)
    {
        var dataFolderPath = Path.Combine(applicationSettings.AppDataRoot, applicationSettings.DataFolderName);
        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        var databasePath = Path.Combine(dataFolderPath, "FileTime.db");
        _liteDb = new LiteDatabase($"Filename={databasePath};Mode=Shared;");
    }

    public ITransaction BeginTransaction()
    {
        _liteDb.BeginTrans();
        var database = new Transaction(_liteDb);

        return database;
    }

    public IQueryCollection<T> GetCollection<T>(string collectionName) 
        => new QueryCollection<T>(_liteDb.GetCollection<T>(collectionName));

    public void Dispose() => _liteDb.Dispose();
}