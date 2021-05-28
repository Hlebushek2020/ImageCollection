using ImageCollection.Classes.ItemMovers;
using ImageCollection.Classes.Settings;
using ImageCollection.Classes.Views;
using System;
using System.Collections.Generic;

namespace ImageCollection.Classes.Collections
{
    public static class CollectionStore
    {
        public const string BaseCollectionName = "Все";
        public const string DataDirectoryName = "DATA-IC";

        public static readonly Guid BaseCollectionId = new Guid(new byte[16] {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        });

        private static readonly Dictionary<string, Collection> actualCollections = new Dictionary<string, Collection>();
        private static readonly HashSet<string> irrelevantCollections = new HashSet<string>();

        public static IEnumerable<string> ActualCollections { get => actualCollections.Keys; }
        public static IEnumerable<string> IrrelevantCollections { get => irrelevantCollections; }
        public static StoreSettings Settings { get; private set; }

        //Collection this[string collection] { get => actualCollections[collection]; }

        public static void MoveCollectionItems(string from, string to, IEnumerable<ListBoxImageItem> items)
        {
            ItemMover itemMover = new ItemMover
            {
                FromCollection = actualCollections[from],
                ToCollection = actualCollections[to]
            };
            foreach (ListBoxImageItem item in items)
            {
                itemMover.Move(item.Path);
            }
            itemMover.FromCollection.IsChanged = true;
            itemMover.ToCollection.IsChanged = true;
        }
    }
}