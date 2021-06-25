using ImageCollection.Classes.Collections;

namespace ImageCollection.Classes.ItemMovers
{
    /// <summary>
    /// Отвечает за перемещение элементов из коллекции, которая будет удалена, в базовую коллекцию
    /// </summary>
    public class RemoveCollectionItemMover : ItemMover
    {
        /// <summary>
        /// Создание "перемещателя"
        /// </summary>
        /// <param name="from">Из</param>
        /// <param name="to">В</param>
        public RemoveCollectionItemMover(Collection from, Collection to) : base(from, to) { }

        /// <summary>
        /// Перемещение заданного элемента
        /// </summary>
        /// <param name="item">Элемент</param>
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

        /// <summary>
        /// Завершение перемещения элементов
        /// </summary>
        public override void EndMoving()
        {
            toCollection.IsChanged = true;
        }
    }
}