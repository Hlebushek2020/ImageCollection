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
                ToCollection.AddNoFlag(item, false, null);
            else
            {
                if (meta.Parent == null)
                    ToCollection.AddNoFlag(item, false, null);
                else
                {
                    if (meta.Parent != ToCollection.Id)
                        ToCollection.AddNoFlag(item, false, null);
                    else
                        ToCollection.AddNoFlag(item, true, null);
                }
            }
        }
    }
}
