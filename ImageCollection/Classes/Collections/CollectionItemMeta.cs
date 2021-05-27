using System;

namespace ImageCollection.Classes.Collections
{
    public class CollectionItemMeta
    {
        public bool InCurrentFolder { get; private set; }
        public Guid? Parent { get; private set; }
        public string Hash { get; set; }
        public string Description { get; set; }
        
        public CollectionItemMeta(bool inCurrentFolder, Guid? parent)
        {
            InCurrentFolder = inCurrentFolder;
            Parent = parent;
        }

        public void Change(bool inCurrentFolder, Guid? parent)
        {
            InCurrentFolder = inCurrentFolder;
            Parent = parent;
        }
    }
}