using System;

namespace ImageCollection.Classes.Collections
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