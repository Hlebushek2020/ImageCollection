using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageCollection.Classes.Converters
{
    public class HotkeyConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Hotkey))
            {
                return true;
            }
            return base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string[] hotkeys = ((string)value).Split('+');
            ModifierKeys modifier = (ModifierKeys)Convert.ToInt32(hotkeys[0]);
            Key key = (Key)Convert.ToInt32(hotkeys[1]);
            return new Hotkey { Modifier = modifier, Key = key };
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertFrom(context, culture, value);
        }
    }
}