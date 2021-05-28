using ImageCollection.Classes.ItemMovers;
using ImageCollection.Classes.Settings;
using ImageCollection.Classes.Views;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;

namespace ImageCollection.Classes.Collections
{
    public static class CollectionStore
    {
        public const string BaseCollectionName = "Все";
        public const string DataDirectoryName = "DATA-IC";

        public static readonly Guid BaseCollectionId = new Guid(new byte[16] {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        });

        private static readonly Dictionary<string, Collection> actualCollections = new Dictionary<string, Collection>();
        private static readonly HashSet<string> irrelevantCollections = new HashSet<string>();

        public static IEnumerable<string> ActualCollections { get => actualCollections.Keys; }
        public static IEnumerable<string> IrrelevantCollections { get => irrelevantCollections; }
        public static StoreSettings Settings { get; private set; }

        /// <summary>
        /// Возвращает коллекцию соответствующую названию
        /// </summary>
        /// <param name="collection">Название коллекции</param>
        public static Collection Get(string collection) =>
            actualCollections[collection];

        /// <summary>
        /// Перемещает элементы из одной коллекции в другую
        /// </summary>
        /// <param name="from">Название коллекции откуда перемещаются элементы</param>
        /// <param name="to">Название коллекции куда перемещаются элементы</param>
        /// <param name="items">Элементы для перемещения</param>
        public static void MoveCollectionItems(string from, string to, IEnumerable<ListBoxImageItem> items)
        {
            ItemMover itemMover = new ItemMover
            {
                FromCollection = actualCollections[from],
                ToCollection = actualCollections[to]
            };
            foreach (ListBoxImageItem item in items)
            {
                itemMover.Move(item.Path);
            }
            itemMover.FromCollection.IsChanged = true;
            itemMover.ToCollection.IsChanged = true;
        }

        /// <summary>
        /// Добавление новой пустой коллекции
        /// </summary>
        /// <param name="information"></param>
        public static void Add(CollectionInformation information)
        {
            Collection collection = new Collection
            {
                IsChanged = true,
                Description = information.Description
            };
            if (irrelevantCollections.Contains(information.Name))
            {
                irrelevantCollections.Remove(information.Name);
            }
            actualCollections.Add(information.Name, collection);
        }

        /// <summary>
        /// Добавление коллекции без соблюдения правил добавления
        /// </summary>
        /// <param name="name">Назвыание коллекции</param>
        /// <param name="collection">Коллекция</param>
        public static void AddIgnorRules(string name, Collection collection)
        {
            if (irrelevantCollections.Contains(name))
            {
                irrelevantCollections.Remove(name);
            }
            actualCollections.Add(name, collection);
        }

        /// <summary>
        /// Удаление коллекции
        /// </summary>
        /// <param name="collection">Название коллекции</param>
        public static void Remove(string collection)
        {
            RemoveCollectionItemMover itemMover = new RemoveCollectionItemMover
            {
                FromCollection = actualCollections[collection],
                ToCollection = actualCollections[BaseCollectionName]
            };
            foreach (KeyValuePair<string, CollectionItemMeta> item in itemMover.FromCollection.ActualItems)
            {
                itemMover.Move(item.Key);
            }
            itemMover.ToCollection.IsChanged = true;
            actualCollections.Remove(collection);
            if (!irrelevantCollections.Contains(collection))
            {
                irrelevantCollections.Add(collection);
            }
        }

        /// <summary>
        /// Редактирует описание и название коллекции
        /// </summary>
        /// <param name="collection">Название редактируемой коллекции</param>
        /// <param name="information">Данные для изменения</param>
        public static void Edit(string collection, CollectionInformation information)
        {
            Collection collectionClass = actualCollections[collection];
            if (information.ChangedDescription)
            {
                collectionClass.Description = information.Description;
            }
            if (information.ChangedName)
            {
                if (irrelevantCollections.Contains(information.Name))
                {
                    irrelevantCollections.Remove(information.Name);
                }
                actualCollections.Remove(collection);
                actualCollections.Add(information.Name, collectionClass);
            }
        }

        /// <summary>
        /// Проверяет существует коллекция с таким названием или нет
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool Contains(string collection)
        {
            return actualCollections.ContainsKey(collection);
        }

        /// <summary>
        /// Очистка списка удаленных коллекций
        /// </summary>
        public static void ClearIrrelevantItems()
        {
            irrelevantCollections.Clear();
        }
    }
}