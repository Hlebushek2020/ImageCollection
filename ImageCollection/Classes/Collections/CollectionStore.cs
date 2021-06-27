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
        private static readonly HashSet<string> irrelevantDistributionCollections = new HashSet<string>();
        private static readonly List<Guid> irrelevantSavedCollections = new List<Guid>();

        public static IEnumerable<string> ActualCollections { get => actualCollections.Keys; }
        public static IEnumerable<string> IrrelevantDistributionCollections { get => irrelevantDistributionCollections; }
        public static IEnumerable<Guid> IrrelevantSavedCollections { get => irrelevantSavedCollections; }
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
        public static void AddIrrelevantDistribution(string collection)
        {
            irrelevantDistributionCollections.Add(collection);
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
            irrelevantSavedCollections.Add(from.Id);
            if (!string.IsNullOrEmpty(from.OriginalFolderName))
            {
                if (!irrelevantDistributionCollections.Contains(from.OriginalFolderName))
                {
                    irrelevantDistributionCollections.Add(from.OriginalFolderName);
                }
            }
            Settings.RemoveHotkey(collection);
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
                Settings.SetHotkeyCollection(collection, information.Name);
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
        public static bool ContainsIrrelevantDistribution(string collection)
        {
            return irrelevantDistributionCollections.Contains(collection);
        }

        /// <summary>
        /// Окончательно удаляет удаленную коллекцию
        /// </summary>
        /// <param name="collection">Оригинальное название коллекции</param>
        public static void RemoveIrrelevantDistribution(string collection)
        {
            irrelevantDistributionCollections.Remove(collection);
        }

        /// <summary>
        /// Очистка списка удаленных коллекций
        /// </summary>
        public static void ClearIrrelevantDistribution()
        {
            irrelevantDistributionCollections.Clear();
        }

        /// <summary>
        /// Подготовка хранилища для работы
        /// </summary>
        public static void Reset(string baseDirectory, bool openCollections = false)
        {
            actualCollections.Clear();
            irrelevantDistributionCollections.Clear();
            irrelevantSavedCollections.Clear();
            if (openCollections)
            {
                Settings = StoreSettings.Load(baseDirectory);
            }
            else
            {
                Settings = new StoreSettings(baseDirectory);
            }
        }

        /// <summary>
        /// Очищает список сохраненных удаленных коллекций
        /// </summary>
        public static void ClearIrrelevantSaved()
        {
            irrelevantSavedCollections.Clear();
        }
    }
}