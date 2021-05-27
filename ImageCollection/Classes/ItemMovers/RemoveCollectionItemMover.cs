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
            if (meta.InCurrentFolder)
                ToCollection.Add(item, false, null, meta.Hash, meta.Description);
            else
            {
                if (meta.Parent == null)
                    ToCollection.Add(item, false, null, meta.Hash, meta.Description);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.Add(item, false, null, meta.Hash, meta.Description);
                    else
                        ToCollection.Add(item, true, null, meta.Hash, meta.Description);
                }
            }
        }
    }
}
