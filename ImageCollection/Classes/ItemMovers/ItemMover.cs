using ImageCollection.Classes.Collections;

namespace ImageCollection.Classes.ItemMovers
{
    /// <summary>
    /// Отвечает за перемещение элементов между коллекциями
    /// </summary>
    public class ItemMover
    {
        public Collection FromCollection;
        public Collection ToCollection;

        public virtual void Move(string item)
        {
            CollectionItemMeta meta = FromCollection[item];
            FromCollection.Remove(item);
            if (meta.InCurrentFolder)
                ToCollection.Add(item, false, FromCollection.Id, meta.Hash, meta.Description);
            else
            {
                if (meta.Parent == null)
                    ToCollection.Add(item, false, null, meta.Hash, meta.Description);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.Add(item, false, meta.Parent, meta.Hash, meta.Description);
                    else
                        ToCollection.Add(item, true, null, meta.Hash, meta.Description);
                }
            }
        }
    }
}
