using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using FileTime.App.Core.Command;

namespace FileTime.Avalonia.Configuration
{
    public class CommandBindingConfiguration
    {
        public List<KeyConfig> Keys { get; set; } = new List<KeyConfig>();

        public Commands Command { get; set; } = Commands.None;

        public string KeysDisplayText => GetKeysDisplayText();

        public CommandBindingConfiguration() { }

        public CommandBindingConfiguration(Commands command, IEnumerable<KeyConfig> keys)
        {
            Keys = new List<KeyConfig>(keys);
            Command = command;
        }

        public CommandBindingConfiguration(Commands command, KeyConfig key)
        {
            Keys = new List<KeyConfig>() { key };
            Command = command;
        }

        public CommandBindingConfiguration(Commands command, IEnumerable<Key> keys)
        {
            Keys = keys.Select(k => new KeyConfig(k)).ToList();
            Command = command;
        }

        public CommandBindingConfiguration(Commands command, Key key)
        {
            Keys = new List<KeyConfig>() { new KeyConfig(key) };
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

        private static string AddKeyWithCtrlOrAlt(KeyConfig key, string currentText, Func<KeyConfig, string, bool, string> keyProcessor)
        {
            var s = "";

            bool ctrlOrAlt = key.Ctrl || key.Alt;

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
}