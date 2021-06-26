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
        public Key Key { get; }
        public string CollectionName { get; }
        public bool IsEdit { get; }

        public CollectionKeyInformation(Key key, string collectionName, bool isEdit)
        {
            Key = key;
            CollectionName = collectionName;
            IsEdit = isEdit;
        }
    }
}
