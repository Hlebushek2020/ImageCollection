using ImageCollection.Classes.Collections;

namespace ImageCollection.Classes.ItemMovers
{
    /// <summary>
    /// Отвечает за перемещение элементов между коллекциями
    /// </summary>
    public class ItemMover
    {
        protected Collection fromCollection;
        protected Collection toCollection;

        /// <param name="from">Коллекция из которой перемещать элементы</param>
        /// <param name="to">Коллекция в которую перемещать элементы</param>
        public ItemMover(Collection from, Collection to)
        {
            fromCollection = from;
            toCollection = to;
        }

        /// <summary>
        /// Перемещает заданный элемент
        /// </summary>
        /// <param name="item">Элемент</param>
        public virtual void Move(string item)
        {
            CollectionItemMeta meta = fromCollection[item];
            fromCollection.Remove(item);
            if (meta.InCurrentFolder)
                toCollection.Add(item, false, fromCollection.Id, meta);
            else
            {
                if (meta.Parent == null)
                    toCollection.Add(item, false, null, meta);
                else
                {
                    if (meta.Parent != toCollection.Id)
                        toCollection.Add(item, false, meta.Parent, meta);
                    else
                        toCollection.Add(item, true, null, meta);
                }
            }
        }

        /// <summary>
        /// Завершает перемещение элементов
        /// </summary>
        public virtual void EndMoving()
        {
            fromCollection.IsChanged = true;
            toCollection.IsChanged = true;
        }
    }
}
