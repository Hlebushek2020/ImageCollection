using ImageCollection.Classes.ItemMovers;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;

namespace ImageCollection.Classes.Static
{
    public static class CollectionStore
    {
        public const string BaseCollectionName = "Все";
        public const string DataDirectoryName = "DATA-IC";
        public const string BaseCollectionGuid = "ffffffff-ffff-ffff-ffff-ffffffffffff";

        public static string BaseDirectory { get; private set; }
        public static string DistributionDirectory { get; private set; }
        public static IEnumerable<string> IrrelevantCollections { get => irrelevantCollections; }

        private static readonly Dictionary<string, Collection> actualCollections = new Dictionary<string, Collection>();
        private static readonly HashSet<string> irrelevantCollections = new HashSet<string>();
        private static ItemMover itemMover;

        /// <summary>
        /// Инициализация хранилища
        /// </summary>
        public static void Init(string baseDirectory, string distributionDirectory)
        {
            BaseDirectory = baseDirectory;
            DistributionDirectory = distributionDirectory;
            irrelevantCollections.Clear();
            actualCollections.Clear();
        }

        /// <summary>
        /// Очистка списка удаленных коллекций
        /// </summary>
        public static void IrrelevantCollectionsClear() => irrelevantCollections.Clear();

        public static void BaseDirectoryFromDistributionDirectory()
        {
            BaseDirectory = DistributionDirectory;
            DistributionDirectory = null;
        }

        /// <summary>
        /// Добавление новой коллекции без изменения состояния (IsChanged)
        /// </summary>
        public static void Add(string collectionName, Collection collection)
        {
            if (irrelevantCollections.Contains(collectionName))
                irrelevantCollections.Remove(collectionName);
            actualCollections.Add(collectionName, collection);
        }

        /// <summary>
        /// Добавление новой пустой коллекции
        /// </summary>
        public static void Add(string collectionName, string collectionDescription)
        {
            if (irrelevantCollections.Contains(collectionName))
                irrelevantCollections.Remove(collectionName);
            actualCollections.Add(collectionName, new Collection
            {
                IsChanged = true,
                Description = collectionDescription
            });
        }

        /// <summary>
        /// Возвращает значение, указывающее, существует данная коллекция или нет
        /// </summary>
        public static bool Contains(string collectionName) => actualCollections.ContainsKey(collectionName);

        /// <summary>
        /// Получение коллекции
        /// </summary>
        public static Collection Get(string collectionName) => actualCollections[collectionName];

        /// <summary>
        /// Получение списка коллекций
        /// </summary>
        public static IEnumerable<string> GetCollectionNames() => actualCollections.Keys;

        #region To Collection
        /// <summary>
        /// Инициализация внутреннего объекта для начала перемещения в коллекцию
        /// </summary>
        /// <param name="fromCollectionName">Имя коллекции из которой будет осуществлятся перемещение</param>
        /// <param name="toCollectionName">Имя коллекции в которую будет осуществлятся перемещение</param>
        public static void BeginMovingItems(string fromCollectionName, string toCollectionName)
        {
            itemMover = new ItemMover
            {
                FromCollection = actualCollections[fromCollectionName],
                ToCollection = actualCollections[toCollectionName]
            };
        }
        /// <summary>
        /// Перемещает заданный объект
        /// </summary>
        public static void MoveItem(string item) => itemMover.Move(item);

        /// <summary>
        /// Завершает перемещение
        /// </summary>
        public static void EndMovingItems()
        {
            itemMover.FromCollection.IsChanged = true;
            itemMover.ToCollection.IsChanged = true;
            itemMover = null;
        }
        #endregion

        /// <summary>
        /// Переименовывает коллекцию
        /// </summary>
        public static void Rename(string oldCollectionName, string newCollectionName)
        {
            if (irrelevantCollections.Contains(newCollectionName))
                irrelevantCollections.Remove(newCollectionName);
            Collection collection = actualCollections[oldCollectionName];
            actualCollections.Remove(oldCollectionName);
            actualCollections.Add(newCollectionName, collection);
        }

        /// <summary>
        /// Удаляет указанную коллеукцию
        /// </summary>
        public static void Remove(string collectionName)
        {
            itemMover = new RemoveCollectionItemMover
            {
                FromCollection = actualCollections[collectionName],
                ToCollection = actualCollections[BaseCollectionName]
            };
            foreach (string item in itemMover.FromCollection.ActualItems)
                itemMover.Move(item);
            itemMover.ToCollection.IsChanged = true;
            itemMover = null;
            actualCollections.Remove(collectionName);
            if (!irrelevantCollections.Contains(collectionName))
                irrelevantCollections.Add(collectionName);
        }

    }
}
