using FileTime.App.Core.Command;
using FileTime.Avalonia.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTime.Avalonia.Command
{
    public class CommandBinding
    {
        private readonly Func<Task> _commandHandler;

        public string Name { get; }
        public Commands? Command { get; }
        public KeyWithModifiers[] Keys { get; }

        public string KeysDisplayText => GetKeysDisplayText();

        public CommandBinding(string name, Commands? command, KeyWithModifiers[] keys, Func<Task> commandHandler)
        {
            _commandHandler = commandHandler;
            Name = name;
            Command = command;
            Keys = keys;
        }
        public async Task InvokeAsync() => await _commandHandler();

        public string GetKeysDisplayText()
        {
            var s = "";

            foreach (var k in Keys)
            {
                var keyString = k.Key.ToString();

                if (keyString.Length == 1)
                {
                    s += AddKeyWithCtrlOrAlt(k, s, (_, _, _) => k.Shift ?? false ? keyString.ToUpper() : keyString.ToLower());
                }
                else
                {
                    s += AddKeyWithCtrlOrAlt(k, s, AddSpecialKey);
                }
            }

            return s;
        }

        private static string AddKeyWithCtrlOrAlt(KeyWithModifiers key, string currentText, Func<KeyWithModifiers, string, bool, string> keyProcessor)
        {
            var s = "";

            bool ctrlOrAlt = (key.Ctrl ?? false) || (key.Alt ?? false);

            if (ctrlOrAlt && currentText.Length > 0 && currentText.Last() != ' ') s += " ";

            if (key.Ctrl ?? false) s += "CTRL+";
            if (key.Alt ?? false) s += "ALT+";
            s += keyProcessor(key, currentText, ctrlOrAlt);

            if (ctrlOrAlt) s += " ";

            return s;
        }

        private static string AddSpecialKey(KeyWithModifiers key, string currentText, bool wasCtrlOrAlt)
        {
            var s = "";

            if (currentText.Length > 0 && currentText.Last() != ' ' && !wasCtrlOrAlt) s += " ";
            s += key.Key.ToString();
            if (!wasCtrlOrAlt) s += " ";

            return s;
        }
    }
}
