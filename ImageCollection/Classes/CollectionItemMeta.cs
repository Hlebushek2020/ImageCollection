using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCollection.Classes
{
    public class CollectionItemMeta
    {
        public bool InCurrentFolder { get; }
        public Guid? Parent { get; }
        public string Hash { get; set; }
        public string Description { get; set; }
        
        public CollectionItemMeta(bool inCurrentFolder, Guid? parent)
        {
            InCurrentFolder = inCurrentFolder;
            Parent = parent;
        }
    }
}