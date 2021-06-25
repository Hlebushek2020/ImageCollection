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

        /// <summary>
        /// Создание "перемещателя"
        /// </summary>
        /// <param name="from">Из</param>
        /// <param name="to">В</param>
        public ItemMover(Collection from, Collection to)
        {
            fromCollection = from;
            toCollection = to;
        }

        /// <summary>
        /// Перемещение заданного элемента
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
        /// Завершение перемещения элементов
        /// </summary>
        public virtual void EndMoving()
        {
            fromCollection.IsChanged = true;
            toCollection.IsChanged = true;
        }
    }
}