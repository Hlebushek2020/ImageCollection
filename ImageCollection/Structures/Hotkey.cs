using ImageCollection.Classes.Converters;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ImageCollection.Structures
{
    [TypeConverter(typeof(HotkeyConverter))]
    public struct Hotkey : IEquatable<Hotkey>
    {
        public ModifierKeys Modifier { get; set; }
        public Key Key { get; set; }

        public string Display
        {
            get
            {
                return (Modifier == ModifierKeys.None) ? Key.ToString() : $"{(Modifier == ModifierKeys.Control ? "Ctrl" : Modifier.ToString())} + {Key}";
            }
        }

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
            return $"{(int)Modifier}+{(int)Key}";
        }
    }
}