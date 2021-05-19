using ImageCollection.Classes.ItemMovers;
using ImageCollection.Structures;

namespace ImageCollection.Classes
{
    /// <summary>
    /// Отвечает за перемещение элементов из коллекции, которая будет удалена, в другую коллекцию
    /// </summary>
    public class RemoveCollectionItemMover : ItemMover
    { 
        public override void Move(string item)
        {
            CollectionItemMeta meta = FromCollection.GetMeta(item);
            if (meta.InCurrentFolder)
                ToCollection.Add(item, false, null);
            else
            {
                if (meta.Parent == null)
                    ToCollection.Add(item, false, null);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.Add(item, false, null);
                    else
                        ToCollection.Add(item, true, null);
                }
            }
        }
    }
}
