using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCollection.Structures
{
    public struct CollectionItemMeta
    {
        public bool InCurrentFolder { get; }
        public Guid? Parent { get; }
        
        public CollectionItemMeta(bool inCurrentFolder, Guid? parent)
        {
            InCurrentFolder = inCurrentFolder;
            Parent = parent;
        }
    }
}