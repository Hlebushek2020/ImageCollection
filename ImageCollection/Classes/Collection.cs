using ImageCollection.Classes.Static;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ImageCollection.Classes
{
    public class Collection
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public IEnumerable<string> ActualItems { get => actualItems.Keys; }
        public IEnumerable<string> IrrelevantItems { get => irrelevantItems; }
        public string Description { get; set; } = string.Empty;
        public bool IsChanged { get; set; } = false;

        private readonly Dictionary<string, CollectionItemMeta> actualItems = new Dictionary<string, CollectionItemMeta>();
        private readonly HashSet<string> irrelevantItems = new HashSet<string>();

        /// <summary>
        /// Создание пустой коллекции
        /// </summary>
        public Collection() { }

        /// <summary>
        /// Добавляет элемент в список актуальных без изменения состояния (IsChanged)
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">В своей папке находится элемент или нет</param>
        /// <param name="fromColltction">Идентификатор папки (коллекции) в которой находится элемент, если не в своей папке (коллекции)</param>
        public void AddNoFlag(string item, bool inCurrentFolder, Guid? parent)
        {
            actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));
            if (inCurrentFolder && irrelevantItems.Contains(item))
                irrelevantItems.Remove(item);
        }

        /// <summary>
        /// Добавляет элемент в список актуальных игнорируя изменение состояния и исключенные элементы
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">В своей папке находится элемент или нет</param>
        /// <param name="fromColltction">Идентификатор папки (коллекции) в которой находится элемент, если не в своей папке (коллекции)</param>
        public void AddIgnoreAll(string item, bool inCurrentFolder, Guid? parent) =>
            actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));

        /// <summary>
        /// Получение метаданных элемента коллекции
        /// </summary>
        public CollectionItemMeta GetMeta(string item) => actualItems[item];

        /// <summary>
        /// Удаляет элемент из актуальных без изменения состояния (IsChanged)
        /// </summary>
        public void RemoveNoFlag(string item)
        {
            CollectionItemMeta itemMeta = actualItems[item];
            if (itemMeta.InCurrentFolder)
                irrelevantItems.Add(item);
            actualItems.Remove(item);
        }

        /// <summary>
        /// Удаляет элемент из актуальных игнорируя изменение состояния и исключенные элементы
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">В своей папке находится элемент или нет</param>
        /// <param name="fromColltction">Идентификатор папки (коллекции) в которой находится элемент, если не в своей папке (коллекции)</param>
        public void RemoveIgnoreAll(string item) =>
            actualItems.Remove(item);

        /// <summary>
        /// Удаляет элемент из актуальных окончательно
        /// </summary>
        public void RemovePermanently(string item)
        {
            actualItems.Remove(item);
            IsChanged = true;
        }

        /// <summary>
        /// Очистка коллекции, которая, указывает какие элементы были исключены из текущей папки коллекции
        /// </summary>
        public void IrrelevantItemsClear()
        {
            if (irrelevantItems.Count != 0)
            {
                irrelevantItems.Clear();
                IsChanged = true;
            }
        }

        /// <summary>
        /// Переименовывает элемент коллекции
        /// </summary>
        public void RenameItem(string oldName, string newName)
        {
            CollectionItemMeta itemMeta = actualItems[oldName];
            actualItems.Remove(oldName);
            actualItems.Add(newName, itemMeta);
            //IsChanged = true;
        }

        ///// <summary>
        ///// Очищает коллекцию
        ///// </summary>
        //public void Clear()
        //{
        //    actualItems.Clear();
        //    irrelevantItems.Clear();
        //}

    }
}
