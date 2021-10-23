using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ImageCollection.Classes.Collections
{
    public class Collection
    {
        private readonly ConcurrentDictionary<string, CollectionItemMeta> actualItems = new ConcurrentDictionary<string, CollectionItemMeta>();
        private readonly HashSet<string> irrelevantItems = new HashSet<string>();

        /// <summary>
        /// Идентификатор коллекции
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Актуальные элементы
        /// </summary>
        public IEnumerable<KeyValuePair<string, CollectionItemMeta>> ActualItems { get => actualItems; }
        /// <summary>
        /// Актуальные элементы (ключи)
        /// </summary>
        public IEnumerable<string> ActualItemsKeys { get => actualItems.Keys; }
        /// <summary>
        /// Исключенные элементы
        /// </summary>
        public IEnumerable<string> IrrelevantItems { get => irrelevantItems; }
        /// <summary>
        /// Название папки на диске
        /// </summary>
        public string OriginalFolderName { get; set; } = string.Empty;
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Изменения
        /// </summary>
        public bool IsChanged { get; set; } = false;

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
        /// Добавление элемента с соблюдением правил добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        public void Add(string item, bool inCurrentFolder, Guid? parent)
        {
            actualItems.TryAdd(item, new CollectionItemMeta(inCurrentFolder, parent));
            //actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));
            if (inCurrentFolder && irrelevantItems.Contains(item))
            {
                irrelevantItems.Remove(item);
            }
        }

        /// <summary>
        /// Добавление элемента с соблюдением правил добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        /// <param name="itemMeta">Старый элемент</param>
        public void Add(string item, bool inCurrentFolder, Guid? parent, CollectionItemMeta itemMeta)
        {
            itemMeta.Change(inCurrentFolder, parent);
            actualItems.TryAdd(item, itemMeta);
            //actualItems.Add(item, itemMeta);
            if (inCurrentFolder && irrelevantItems.Contains(item))
            {
                irrelevantItems.Remove(item);
            }
        }

        /// <summary>
        /// Добавление элемента без соблюдения правил добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        public void AddIgnorRules(string item, bool inCurrentFolder, Guid? parent)
        {
            actualItems.TryAdd(item, new CollectionItemMeta(inCurrentFolder, parent));
            //actualItems.Add(item, new CollectionItemMeta(inCurrentFolder, parent));
        }

        /// <summary>
        /// Добавление элемента без соблюдения правил добавления
        /// </summary>
        /// <param name="item">Элемент</param>
        /// <param name="inCurrentFolder">Если True элемент в паке коллекции</param>
        /// <param name="parent">Родительская коллекция</param>
        /// <param name="itemMeta">Старый элемент</param>
        public void AddIgnorRules(string item, bool inCurrentFolder, Guid? parent, CollectionItemMeta itemMeta)
        {
            itemMeta.Change(inCurrentFolder, parent);
            actualItems.TryAdd(item, itemMeta);
            //actualItems.Add(item, itemMeta);
        }

        /// <summary>
        /// Добавление элемента в список удаленных
        /// </summary>
        /// <param name="item">Элемент</param>
        public void AddIrrelevantItem(string item)
        {
            irrelevantItems.Add(item);
        }

        /// <summary>
        /// Очистка коллекции, указывающей, какие элементы были исключены из актуальных
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
        /// Удаление элемента с соблюдением правил удаления
        /// </summary>
        /// <param name="item">Элемент</param>
        public void Remove(string item)
        {
            CollectionItemMeta itemMeta = actualItems[item];
            if (itemMeta.InCurrentFolder)
            {
                irrelevantItems.Add(item);
            }
            actualItems.TryRemove(item, out _);
            //actualItems.Remove(item);
        }

        /// <summary>
        /// Удаление элемента без соблюдения правил удаления
        /// </summary>
        /// <param name="item">Элемент</param>
        public void RemoveIgnorRules(string item) =>
            actualItems.TryRemove(item, out _);
        //actualItems.Remove(item);


        /// <summary>
        /// Переименовыание элемента коллекции
        /// </summary>
        /// <param name="oldName">Старое имя</param>
        /// <param name="newName">Новое имя</param>
        public void Rename(string oldName, string newName)
        {
            //actualItems[oldName];
            actualItems.TryRemove(oldName, out CollectionItemMeta itemMeta);
            actualItems.TryAdd(newName, itemMeta);
            //actualItems.Remove(oldName);
            //actualItems.Add(newName, itemMeta);
        }
    }
}