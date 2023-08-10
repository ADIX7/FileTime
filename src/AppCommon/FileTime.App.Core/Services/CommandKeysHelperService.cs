using System.Text;
using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public class CommandKeysHelperService : ICommandKeysHelperService
{
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;

    public CommandKeysHelperService(IKeyboardConfigurationService keyboardConfigurationService)
    {
        _keyboardConfigurationService = keyboardConfigurationService;
    }

    public List<List<KeyConfig>> GetKeysForCommand(string commandName)
        => _keyboardConfigurationService
            .AllShortcut
            .Where(s => s.Command == commandName)
            .Select(k => k.Keys)
            .ToList();


    public string GetKeyConfigsString(string commandIdentifier)
    {
        var keyConfigs = GetKeysForCommand(commandIdentifier);
        if (keyConfigs.Count == 0) return string.Empty;

        return string.Join(
            " ; ",
            keyConfigs
                .Select(ks =>
                    string.Join(
                        ", ",
                        ks.Select(FormatKeyConfig)
                    )
                )
        );
    }

    public string FormatKeyConfig(KeyConfig keyConfig)
    {
        var stringBuilder = new StringBuilder();

        if (keyConfig.Ctrl) stringBuilder.Append("Ctrl + ");
        if (keyConfig.Shift) stringBuilder.Append("Shift + ");
        if (keyConfig.Alt) stringBuilder.Append("Alt + ");

        stringBuilder.Append(keyConfig.Key.ToString());

        return stringBuilder.ToString();
    }
}