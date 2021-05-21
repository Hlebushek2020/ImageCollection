using ImageCollection.Structures;

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
                ToCollection.Add(item, false, FromCollection.Id);
            else
            {
                if (meta.Parent == null)
                    ToCollection.Add(item, false, null);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.Add(item, false, meta.Parent);
                    else
                        ToCollection.Add(item, true, null);
                }
            }
        }
    }
}
