using FileTime.App.Core.Models;
using GeneralInputKey;

namespace FileTime.App.Core.Configuration;

public class CommandBindingConfiguration
{
    public List<KeyConfig> Keys { get; set; }

    public string Command { get; set; }

    public string KeysDisplayText => GetKeysDisplayText();

    public CommandBindingConfiguration()
    {
        Command = null!;
        Keys = null!;
    }

    public CommandBindingConfiguration(string command, IEnumerable<KeyConfig> keys)
    {
        Keys = new List<KeyConfig>(keys);
        Command = command;
    }

    public CommandBindingConfiguration(string command, KeyConfig key)
    {
        Keys = new List<KeyConfig> { key };
        Command = command;
    }

    public CommandBindingConfiguration(string command, IEnumerable<Keys> keys)
    {
        Keys = keys.Select(k => new KeyConfig(k)).ToList();
        Command = command;
    }

    public CommandBindingConfiguration(string command, Keys key)
    {
        Keys = new List<KeyConfig>() { new(key) };
        Command = command;
    }

    public string GetKeysDisplayText()
    {
        var s = "";

        foreach (var k in Keys)
        {
            var keyString = k.Key.ToString();

            if (keyString.Length == 1)
            {
                s += AddKeyWithCtrlOrAlt(k, s, (_, _, _) => k.Shift ? keyString.ToUpper() : keyString.ToLower());
            }
            else
            {
                s += AddKeyWithCtrlOrAlt(k, s, AddSpecialKey);
            }
        }

        return s;
    }

    private static string AddKeyWithCtrlOrAlt(
        KeyConfig key,
        string currentText,
        Func<KeyConfig, string, bool, string> keyProcessor)
    {
        var s = "";

        var ctrlOrAlt = key.Ctrl || key.Alt;

        if (ctrlOrAlt && currentText.Length > 0 && currentText.Last() != ' ') s += " ";

        if (key.Ctrl) s += "CTRL+";
        if (key.Alt) s += "ALT+";
        s += keyProcessor(key, currentText, ctrlOrAlt);

        if (ctrlOrAlt) s += " ";

        return s;
    }

    private static string AddSpecialKey(KeyConfig key, string currentText, bool wasCtrlOrAlt)
    {
        var s = "";

        if (currentText.Length > 0 && currentText.Last() != ' ' && !wasCtrlOrAlt) s += " ";
        s += key.Key.ToString();
        if (!wasCtrlOrAlt) s += " ";

        return s;
    }
}