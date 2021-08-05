using System;
using System.Windows.Input;

namespace ImageCollection.Structures
{
    public struct Hotkey : IEquatable<Hotkey>
    {
        public ModifierKeys Modifier { get; set; }
        public Key Key { get; set; }

        public bool Equals(Hotkey other)
        {
            return (Modifier == other.Modifier) && (Key == other.Key);
        }

        public override int GetHashCode()
        {
            return (Modifier.ToString() + Key.ToString()).GetHashCode();
        }

        public override string ToString()
        {
            return (Modifier == ModifierKeys.None) ? Key.ToString() : $"{(Modifier == ModifierKeys.Control ? "Ctrl" : Modifier.ToString())} + {Key}";
        }
    }
}