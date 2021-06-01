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
        public const string PreviewDirectoryName = "preview";

        public static Guid BaseCollectionId { get; } = new Guid(new byte[16] {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        });

        private static readonly Dictionary<string, Collection> actualCollections = new Dictionary<string, Collection>();
        private static readonly Dictionary<string, Guid?> irrelevantCollections = new Dictionary<string, Guid?>();

        public static IEnumerable<string> ActualCollections { get => actualCollections.Keys; }
        public static IEnumerable<KeyValuePair<string, Guid?>> IrrelevantCollections { get => irrelevantCollections; }
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
        public static ItemMover InitializeItemMover(string from, string to)
        {
            return new ItemMover(actualCollections[from], actualCollections[to]);
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
            //if (irrelevantCollections.ContainsKey(information.Name))
            //{
            //    irrelevantCollections.Remove(information.Name);
            //}
            actualCollections.Add(information.Name, collection);
        }

        /// <summary>
        /// Добавление коллекции без соблюдения правил добавления
        /// </summary>
        /// <param name="name">Назвыание коллекции</param>
        /// <param name="collection">Коллекция</param>
        public static void AddIgnorRules(string name, Collection collection)
        {
            //if (irrelevantCollections.ContainsKey(name))
            //{
            //    irrelevantCollections.Remove(name);
            //}
            actualCollections.Add(name, collection);
        }

        /// <summary>
        /// Добавление удаленной коллекции
        /// </summary>
        /// <param name="collection">Название коллекции</param>
        /// <param name="id">Guid коллекции</param>
        public static void AddIrrelevant(string collection, Guid? id = null)
        {
            irrelevantCollections.Add(collection, id);
        }

        /// <summary>
        /// Удаление коллекции
        /// </summary>
        /// <param name="collection">Название коллекции</param>
        public static void Remove(string collection)
        {
            Collection from = actualCollections[collection];
            ItemMover itemMover = new RemoveCollectionItemMover(from, actualCollections[BaseCollectionName]);
            foreach (KeyValuePair<string, CollectionItemMeta> item in from.ActualItems)
            {
                itemMover.Move(item.Key);
            }
            itemMover.EndMoving();
            actualCollections.Remove(collection);
            if (!string.IsNullOrEmpty(from.OriginalFolderName))
            {
                if (!irrelevantCollections.ContainsKey(from.OriginalFolderName))
                {
                    irrelevantCollections.Add(from.OriginalFolderName, from.Id);
                }
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
                //if (irrelevantCollections.ContainsKey(information.Name))
                //{
                //    irrelevantCollections.Remove(information.Name);
                //}
                actualCollections.Remove(collection);
                actualCollections.Add(information.Name, collectionClass);
            }
            collectionClass.IsChanged = collectionClass.IsChanged || information.ChangedDescription || information.ChangedName;
        }

        /// <summary>
        /// Наличие коллекции в актуальных
        /// </summary>
        /// <param name="collection">Название коллекции</param>
        public static bool Contains(string collection)
        {
            return actualCollections.ContainsKey(collection);
        }

        /// <summary>
        /// Наличие коллекции в удаленных
        /// </summary>
        /// <param name="collection">Оригинальное название коллекции</param>
        public static bool ContainsIrrelevant(string collection)
        {
            return irrelevantCollections.ContainsKey(collection);
        }

        /// <summary>
        /// Окончательно удаляет удаленную коллекцию
        /// </summary>
        /// <param name="collection">Оригинальное название коллекции</param>
        public static void RemoveIrrelevant(string collection)
        {
            irrelevantCollections.Remove(collection);
        }

        /// <summary>
        /// Очистка списка удаленных коллекций
        /// </summary>
        public static void ClearIrrelevantItems()
        {
            irrelevantCollections.Clear();
        }

        /// <summary>
        /// Подготовка хранилища для работы
        /// </summary>
        public static void Reset(string baseDirectory, bool openCollections = false)
        {
            actualCollections.Clear();
            irrelevantCollections.Clear();
            if (openCollections)
            {
                Settings = StoreSettings.Load(baseDirectory);
            }
            else
            {
                Settings = new StoreSettings(baseDirectory);
            }
        }
    }
}