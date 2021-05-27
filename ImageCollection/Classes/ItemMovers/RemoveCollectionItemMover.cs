using ImageCollection.Classes.Collections;

namespace ImageCollection.Classes.ItemMovers
{
    /// <summary>
    /// Отвечает за перемещение элементов из коллекции, которая будет удалена, в другую коллекцию
    /// </summary>
    public class RemoveCollectionItemMover : ItemMover
    {
        public override void Move(string item)
        {
            CollectionItemMeta meta = FromCollection[item];
            if (!meta.InCurrentFolder && meta.Parent != null)
            {
                if (meta.Parent == ToCollection.Id)
                {
                    ToCollection.Add(item, true, null, meta);
                    return;
                }
            }
            ToCollection.Add(item, false, null, meta);
        }
    }
}
