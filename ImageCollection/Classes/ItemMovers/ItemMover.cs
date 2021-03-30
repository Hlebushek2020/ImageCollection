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
            CollectionItemMeta meta = FromCollection.GetMeta(item);
            FromCollection.RemoveNoFlag(item);
            if (meta.InCurrentFolder)
                ToCollection.AddNoFlag(item, false, FromCollection.Id);
            else
            {
                if (meta.Parent == null)
                    ToCollection.AddNoFlag(item, false, null);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.AddNoFlag(item, false, meta.Parent);
                    else
                        ToCollection.AddNoFlag(item, true, null);
                }
            }
        }
    }
}
