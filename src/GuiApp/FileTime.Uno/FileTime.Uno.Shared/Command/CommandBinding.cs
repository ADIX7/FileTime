using FileTime.App.Core.Command;
using FileTime.Uno.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileTime.Uno.Command
{
    public class CommandBinding
    {
        private readonly Action _commandHandler;

        public string Name { get; }
        public Commands? Command { get; }
        public KeyWithModifiers[] Keys { get; }
        public void Invoke() => _commandHandler();

        public string KeysDisplayText => GetKeysDisplayText();

        public CommandBinding(string name, Commands? command, KeyWithModifiers[] keys, Action commandHandler)
        {
            _commandHandler = commandHandler;
            Name = name;
            Command = command;
            Keys = keys;
        }

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

            if (ctrlOrAlt && currentText.Last() != ' ') s += " ";

            if (key.Ctrl ?? false) s += "CTRL+";
            if (key.Alt ?? false) s += "ALT+";
            s += keyProcessor(key, currentText, ctrlOrAlt);

            if (ctrlOrAlt) s += " ";

            return s;
        }

        private static string AddSpecialKey(KeyWithModifiers key, string currentText, bool wasCtrlOrAlt)
        {
            var s = "";

            if (currentText.Last() != ' ' && !wasCtrlOrAlt) s += " ";
            s += key.Key.ToString();
            if (!wasCtrlOrAlt) s += " ";

            return s;
        }
    }
}
