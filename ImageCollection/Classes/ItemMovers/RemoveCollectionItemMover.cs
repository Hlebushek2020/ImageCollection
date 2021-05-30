using ImageCollection.Classes.Collections;

namespace ImageCollection.Classes.ItemMovers
{
    /// <summary>
    /// Отвечает за перемещение элементов из коллекции, которая будет удалена, в другую коллекцию
    /// </summary>
    public class RemoveCollectionItemMover : ItemMover
    {
        public RemoveCollectionItemMover(Collection from, Collection to) : base(from, to) { }

        public override void Move(string item)
        {
            CollectionItemMeta meta = fromCollection[item];
            if (!meta.InCurrentFolder && meta.Parent != null)
            {
                if (meta.Parent == toCollection.Id)
                {
                    toCollection.Add(item, true, null, meta);
                    return;
                }
            }
            toCollection.Add(item, false, null, meta);
        }

        public override void EndMoving()
        {
            toCollection.IsChanged = true;
        }
    }
}
