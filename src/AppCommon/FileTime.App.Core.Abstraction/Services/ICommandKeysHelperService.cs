using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public interface ICommandKeysHelperService
{
    List<List<KeyConfig>> GetKeysForCommand(string commandName);
    string GetKeyConfigsString(string commandIdentifier);
    string FormatKeyConfig(KeyConfig keyConfig);
}