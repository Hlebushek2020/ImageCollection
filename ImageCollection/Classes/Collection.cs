using ImageCollection.Structures;
using System;
using System.Collections.Generic;

namespace ImageCollection.Classes
{
    public class Collection : Interfaces.ICollection<CollectionItemMeta>
    {
        private readonly Dictionary<string, CollectionItemMeta> actualItems = new Dictionary<string, CollectionItemMeta>();
        private readonly HashSet<string> irrelevantItems = new HashSet<string>();

        public Guid Id { get; private set; }
        public IEnumerable<string> ActualItems { get => actualItems.Keys; }
        public IEnumerable<string> IrrelevantItems { get => irrelevantItems; }
        public string Description { get; set; }
        public bool IsChanged { get; set; }

        public Collection() =>
            Id = Guid.NewGuid();

        public Collection(Guid id) =>
            Id = id;

        /// <summary>
        /// Получение метаданных элемента коллекции
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <returns></returns>
        public CollectionItemMeta this[string item] { get => actualItems[item]; }

        /// <summary>
        /// Добавляет элемент в коллекцию соблюдая правила добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        public void Add(string item, bool inCurrentFolder, Guid? parent)
        {
            actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));
            if (inCurrentFolder && irrelevantItems.Contains(item))
            {
                irrelevantItems.Remove(item);
            }
        }

        /// <summary>
        /// Добавляет элемент в коллекцию без соблюдения правил добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        public void AddIgnorRules(string item, bool inCurrentFolder, Guid? parent) =>
            actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));

        /// <summary>
        /// Очистка коллекции, указывающей, какие элементы были исключены из текущей папки коллекции
        /// </summary>
        public bool ClearIrrelevantItems()
        {
            if (irrelevantItems.Count != 0)
            {
                irrelevantItems.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Удаляет элемент из коллекции соблюдая правила удаления
        /// </summary>
        /// <param name="item">Элемент</param>
        public void Remove(string item)
        {
            CollectionItemMeta itemMeta = actualItems[item];
            if (itemMeta.InCurrentFolder)
            {
                irrelevantItems.Add(item);
            }
            actualItems.Remove(item);
        }

        /// <summary>
        /// Удаляет элемент из коллекции без соблюдения правил удаления
        /// </summary>
        /// <param name="item">Элемент</param>
        public void RemoveIgnorRules(string item) =>
            actualItems.Remove(item);


        /// <summary>
        /// Переименовывает элемент коллекции
        /// </summary>
        /// <param name="oldName">Старое имя</param>
        /// <param name="newName">Новое имя</param>
        public void Rename(string oldName, string newName)
        {
            CollectionItemMeta itemMeta = actualItems[oldName];
            actualItems.Remove(oldName);
            actualItems.Add(newName, itemMeta);
        }
    }
}