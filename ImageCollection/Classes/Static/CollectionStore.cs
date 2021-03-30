using ImageCollection.Structures;
using System.Collections.Generic;

namespace ImageCollection.Classes.Static
{
    public static class CollectionStore
    {
        public const string BaseCollectionName = "Все";
        public const string DataDirectoryName = "DATA-IC";

        public static string BaseDirectory { get; private set; }
        public static string DistributionDirectory { get; private set; }
        public static IEnumerable<string> IrrelevantCollections { get => irrelevantCollections; }

        private static Dictionary<string, Collection> actualCollections = new Dictionary<string, Collection>();
        private static HashSet<string> irrelevantCollections = new HashSet<string>();

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

        /// <summary>
        /// Перемещение элемента(ов) в другую коллекцию
        /// </summary>
        /// <param name="fromCollectionName">Откуда</param>
        /// <param name="toCollectionName">Куда</param>
        /// <param name="items">"Элементы</param>
        public static void ToCollection(string fromCollectionName, string toCollectionName, IEnumerable<string> items)
        {
            Collection fromCollection = actualCollections[fromCollectionName];
            Collection toCollection = actualCollections[toCollectionName];
            foreach (string item in items)
            {
                CollectionItemMeta meta = fromCollection.GetMeta(item);
                fromCollection.RemoveNoFlag(item);
                if (meta.InCurrentFolder)
                    toCollection.AddNoFlag(item, false, fromCollection.Guid);
                else
                {
                    if (meta.Parent == null)
                        toCollection.AddNoFlag(item, false, null);
                    else
                    {
                        if (meta.Parent != toCollection.Guid)
                            toCollection.AddNoFlag(item, false, meta.Parent);
                        else
                            toCollection.AddNoFlag(item, true, null);
                    }
                }
            }
            fromCollection.IsChanged = true;
            toCollection.IsChanged = true;
        }

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
            Collection fromCollection = actualCollections[collectionName];
            Collection toCollection = actualCollections[BaseCollectionName];
            foreach (string item in fromCollection.ActualItems)
            {
                CollectionItemMeta meta = fromCollection.GetMeta(item);
                if (meta.InCurrentFolder)
                    toCollection.AddNoFlag(item, false, null);
                else
                {
                    if (meta.Parent == null)
                        toCollection.AddNoFlag(item, false, null);
                    else
                    {
                        if (meta.Parent != toCollection.Guid)
                            toCollection.AddNoFlag(item, false, null);
                        else
                            toCollection.AddNoFlag(item, true, null);
                    }
                }
            }
            toCollection.IsChanged = true;
            actualCollections.Remove(collectionName);
            if (!irrelevantCollections.Contains(collectionName))
                irrelevantCollections.Add(collectionName);
        }

    }
}
