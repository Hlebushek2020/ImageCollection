using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageCollection.Structures
{
    public struct CollectionKeyInformation
    {
        public Hotkey Hotkey { get; }
        public string CollectionName { get; }

        public CollectionKeyInformation(Hotkey hotkey, string collectionName)
        {
            Hotkey = hotkey;
            CollectionName = collectionName;
        }
    }
}
