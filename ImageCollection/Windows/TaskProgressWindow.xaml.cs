using ImageCollection.Classes.Collections;
using ImageCollection.Classes.Settings;
using ImageCollection.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для TaskProgressWindow.xaml
    /// </summary>
    public partial class TaskProgressWindow : Window
    {
        private readonly Paragraph logParagraph = new Paragraph();
        private bool inProgress = false;
        private readonly Task currentOperation;

        public TaskProgressWindow(TaskType taskType, object[] args = null)
        {
            InitializeComponent();
            Title = App.Name;
            FlowDocument flowDocument = new FlowDocument();
            flowDocument.Blocks.Add(logParagraph);
            richTextBox_Log.Document = flowDocument;
            switch (taskType)
            {
                case TaskType.OpenCollections:
                    currentOperation = new Task(() => OpenCollectionsTaskAction((string)args[0]));
                    break;
                case TaskType.RenameCollectionItems:
                    currentOperation = new Task(() => RenameAllItemsInCollectionTaskAction((string)args[0], (string)args[1]));
                    break;
                case TaskType.SaveCollections:
                    currentOperation = new Task(() => SaveCollectionsTaskAction());
                    break;
                case TaskType.OpenFolder:
                    currentOperation = new Task(() => OpenFolderTaskAction((string)args[0], (SearchOption)args[1], (string)args[2], (string)args[3]));
                    break;
                case TaskType.Distribution:
                    if (string.IsNullOrEmpty(CollectionStore.Settings.DistributionDirectory))
                    {
                        currentOperation = new Task(() => StdDistributionTaskAction());
                    }
                    else
                    {
                        currentOperation = new Task(() => DistributionTaskAction());
                    }
                    break;
                case TaskType.СlearImageCache:
                    currentOperation = new Task(() => СlearImageCacheAction());
                    break;
                case TaskType.MergeCollections:
                    currentOperation = new Task(() => MergeCollectionsTaskAction((string)args[0]));
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => currentOperation.Start();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = inProgress;

        #region Task Actions
        /// <summary>
        /// Очистка кеша изображений
        /// </summary>
        private void СlearImageCacheAction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Очистка...\r\n");
                });
                string previewDirectory = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
                if (Directory.Exists(previewDirectory))
                {
                    Directory.Delete(previewDirectory, true);
                }
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Сохранение коллекций
        /// </summary>
        private void SaveCollectionsTaskAction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                });
                BaseSaveCollectionsTaskAction();
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Сохранение коллекций [ОБОЛОЧКА]
        /// </summary>
        private void BaseSaveCollectionsTaskAction()
        {
            string metaDirectory = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName);
            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка удаленных коллекций...\r\n"));
            if (!Directory.Exists(metaDirectory))
            {
                Directory.CreateDirectory(metaDirectory);
            }
            else
            {
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Удаление ненужных метаданных...\r\n"));
                foreach (Guid deleteSavedCollection in CollectionStore.IrrelevantSavedCollections)
                {
                    string icdPath = Path.Combine(metaDirectory, $"{deleteSavedCollection}.icd");
                    if (File.Exists(icdPath))
                    {
                        Dispatcher.Invoke((Action<string>)((string _icdPath) => logParagraph.Inlines.Add($"Удаление метаданных: \"{_icdPath}\"\r\n")), icdPath);
                        File.Delete(icdPath);
                    }
                }
                CollectionStore.ClearIrrelevantSaved();
            }
            string dicdFilePath = Path.Combine(metaDirectory, $"irrelevant.icdd");
            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Запись названий удаленных распределенных коллекций...\r\n"));
            using (FileStream dicdFile = new FileStream(dicdFilePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter dicdWriter = new BinaryWriter(dicdFile, Encoding.UTF8))
                {
                    foreach (string deleteCollection in CollectionStore.IrrelevantDistributionCollections)
                    {
                        dicdWriter.Write(deleteCollection);
                    }
                }
            }
            Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Обработка актуальных коллекций...\r\n"));
            foreach (string collectionName in CollectionStore.ActualCollections)
            {
                Dispatcher.Invoke((Action<string>)((string _collection) => logParagraph.Inlines.Add($"Обработка коллекции: \"{_collection}\"\r\n")), collectionName);
                Collection collection = CollectionStore.Get(collectionName);
                if (collection.IsChanged)
                {
                    string icdFilePath = Path.Combine(metaDirectory, $"{collection.Id}.icd");
                    using (FileStream icdFile = new FileStream(icdFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter icdWriter = new BinaryWriter(icdFile, Encoding.UTF8))
                        {
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Запись базовых сведений...\r\n"));
                            icdWriter.Write(collection.Id.ToByteArray());
                            icdWriter.Write(collectionName);
                            bool isValue = !string.IsNullOrEmpty(collection.Description);
                            icdWriter.Write(isValue);
                            if (isValue)
                            {
                                icdWriter.Write(collection.Description);
                            }
                            isValue = !string.IsNullOrEmpty(collection.OriginalFolderName);
                            icdWriter.Write(isValue);
                            if (isValue)
                            {
                                icdWriter.Write(collection.OriginalFolderName);
                            }
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Запись актуальных элементов...\r\n"));
                            foreach (KeyValuePair<string, CollectionItemMeta> item in collection.ActualItems)
                            {
                                if (item.Value.InCurrentFolder)
                                {
                                    continue;
                                }
                                // add item flag
                                icdWriter.Write(true);
                                icdWriter.Write(item.Value.Parent.HasValue);
                                if (item.Value.Parent.HasValue)
                                {
                                    icdWriter.Write(item.Value.Parent.Value.ToByteArray());
                                }
                                icdWriter.Write(item.Key);
                            }
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Запись исключенных элементов...\r\n"));
                            foreach (string deleteItem in collection.IrrelevantItems)
                            {
                                // delete item flag
                                icdWriter.Write(false);
                                icdWriter.Write(deleteItem);
                            }
                            collection.IsChanged = false;
                        }
                    }
                }
            }
            if (CollectionStore.Settings.IsChanged)
            {
                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Сохранение настроек...\r\n"));
                CollectionStore.Settings.Save();
            }
            ProgramSettings settings = ProgramSettings.GetInstance();
            if (!settings.LastOpenCollection.Equals(CollectionStore.Settings.BaseDirectory))
            {
                settings.LastOpenCollection = CollectionStore.Settings.BaseDirectory;
                settings.Save();
            }
        }

        /// <summary>
        /// Чтение сохраненных коллекций
        /// </summary>
        /// <param name="baseDirectory">Базовая директория</param>
        private void OpenCollectionsTaskAction(string baseDirectory)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Поиск метаданных коллекций...\r\n");
                });
                string metaDirectory = Path.Combine(baseDirectory, CollectionStore.DataDirectoryName);
                IEnumerable<string> icdFiles = new DirectoryInfo(metaDirectory)
                    .EnumerateFiles()
                    .Where(x => x.Extension.Equals(".icd"))
                    .Select(x => x.Name);
                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Подготовка хранилища...\r\n"));
                CollectionStore.Reset(baseDirectory, true);
                foreach (string icdFileName in icdFiles)
                {
                    string icdFilePath = Path.Combine(metaDirectory, icdFileName);
                    Dispatcher.Invoke((Action<string>)((string _icdFilePath) => logParagraph.Inlines.Add($"Обработка: \"{_icdFilePath}\"\r\n")), icdFilePath);
                    Collection collection = null;
                    using (FileStream icdFile = new FileStream(icdFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader icdReader = new BinaryReader(icdFile, Encoding.UTF8))
                        {
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Чтение базовых сведений...\r\n"));
                            Guid id = new Guid(icdReader.ReadBytes(16));
                            collection = new Collection(id);
                            string collectionName = icdReader.ReadString();
                            if (icdReader.ReadBoolean())
                            {
                                collection.Description = icdReader.ReadString();
                            }
                            bool ofnContains = icdReader.ReadBoolean();
                            if (ofnContains || id == CollectionStore.BaseCollectionId)
                            {
                                if (ofnContains)
                                {
                                    collection.OriginalFolderName = icdReader.ReadString();
                                }
                                // get files from folder and add coll
                                string collectionFolder = baseDirectory;
                                if (id != CollectionStore.BaseCollectionId)
                                {
                                    collectionFolder = Path.Combine(baseDirectory, collection.OriginalFolderName);
                                }
                                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Получение файлов коллекции...\r\n"));
                                IEnumerable<string> files = new DirectoryInfo(collectionFolder)
                                    .EnumerateFiles()
                                    .Where(x => x.Extension.Equals(".bmp") || x.Extension.Equals(".jpg") || x.Extension.Equals(".jpeg") || x.Extension.Equals(".png"))
                                    .Select(x => x.FullName);
                                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Добавление файлов...\r\n"));
                                foreach (string filePath in files)
                                {
                                    collection.AddIgnorRules(filePath.Remove(0, baseDirectory.Length + 1), true, null);
                                }
                            }
                            // read icd processing
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка актуальных и исключенных элементов...\r\n"));
                            while (icdFile.Length != icdFile.Position)
                            {
                                // add
                                if (icdReader.ReadBoolean())
                                {
                                    Guid? parentId = null;
                                    // contains guid
                                    if (icdReader.ReadBoolean())
                                    {
                                        parentId = new Guid(icdReader.ReadBytes(16));
                                    }
                                    string item = icdReader.ReadString();
                                    collection.AddIgnorRules(item, false, parentId);
                                }
                                // remove
                                else
                                {
                                    string item = icdReader.ReadString();
                                    collection.Remove(item);
                                }
                            }
                            Dispatcher.Invoke((Action<string>)((string _collectionName) => logParagraph.Inlines.Add($"Добавление коллекции \"{_collectionName}\" в хранилище...\r\n")), collectionName);
                            CollectionStore.AddIgnorRules(collectionName, collection);
                        }
                    }
                }
                // read deleted collection
                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Чтение удаленных коллекций...\r\n"));
                string dicdFilePath = Path.Combine(metaDirectory, $"irrelevant.icdd");
                if (File.Exists(dicdFilePath))
                {
                    using (FileStream dicdFile = new FileStream(dicdFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader dicdReader = new BinaryReader(dicdFile, Encoding.UTF8))
                        {
                            while (dicdFile.Length != dicdFile.Position)
                            {
                                string deleteCollectionName = dicdReader.ReadString();
                                CollectionStore.AddIrrelevantDistribution(deleteCollectionName);
                            }
                        }
                    }
                }
                ProgramSettings settings = ProgramSettings.GetInstance();
                settings.LastOpenCollection = baseDirectory;
                settings.Save();
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Открытие папки как коллекция
        /// </summary>
        /// <param name="baseDirectory">Базовая директория</param>
        /// <param name="search">Тип поиска</param>
        /// <param name="searchMask">Маска поиска</param>
        /// <param name="distributionDirectory">Директория для первого распределения</param>
        private void OpenFolderTaskAction(string baseDirectory, SearchOption search, string searchMask, string distributionDirectory)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Подготовка хранилища...\r\n");
                });
                CollectionStore.Reset(baseDirectory);
                CollectionStore.Settings.DistributionDirectory = distributionDirectory;
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Создание базовой коллекции...\r\n"));
                Collection collection = new Collection(CollectionStore.BaseCollectionId);
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Получение списка файлов...\r\n"));
                IEnumerable<FileInfo> files = new DirectoryInfo(baseDirectory)
                    .EnumerateFiles(searchMask, search)
                    .Where(f => f.Extension.Equals(".bmp") || f.Extension.Equals(".jpg") || f.Extension.Equals(".jpeg") || f.Extension.Equals(".png"));
                int deleteCount = baseDirectory.Length + 1;
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка списка файлов...\r\n"));
                foreach (FileInfo fileInfo in files)
                {
                    if (fileInfo.DirectoryName.Equals(baseDirectory))
                    {
                        collection.AddIgnorRules(fileInfo.Name, true, null);
                    }
                    else
                    {
                        collection.AddIgnorRules(fileInfo.FullName.Remove(0, deleteCount), false, null);
                    }
                }
                collection.IsChanged = true;
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Добавление коллекции в хранилище...\r\n"));
                CollectionStore.AddIgnorRules(CollectionStore.BaseCollectionName, collection);
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Распределяет файлы по папкам (используя новое место для хранения)
        /// </summary>
        private void DistributionTaskAction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                });
                string distributionDirectory = CollectionStore.Settings.DistributionDirectory;
                string baseDirectory = CollectionStore.Settings.BaseDirectory;
                string dataDirectory = Path.Combine(baseDirectory, CollectionStore.DataDirectoryName);
                foreach (string collectionName in CollectionStore.ActualCollections)
                {
                    Dispatcher.Invoke((Action<string>)((string _collection) => logParagraph.Inlines.Add($"Обработка коллекции: \"{_collection}\"\r\n")), collectionName);
                    Collection collection = CollectionStore.Get(collectionName);
                    string prefixPath = string.Empty;
                    if (collection.Id != CollectionStore.BaseCollectionId)
                    {
                        prefixPath = collectionName;
                    }
                    string toPath = Path.Combine(distributionDirectory, prefixPath);
                    Directory.CreateDirectory(toPath);
                    List<string> actualItemsTemp = new List<string>(collection.ActualItemsKeys);
                    foreach (string item in actualItemsTemp)
                    {
                        string fromFilePath = Path.Combine(baseDirectory, item);
                        Dispatcher.Invoke((Action<string>)((string _fromFilePath) => logParagraph.Inlines.Add($"Подготовка и копирование: \"{_fromFilePath}\"\r\n")), fromFilePath);
                        string fromFileName = Path.GetFileNameWithoutExtension(item);
                        string fromFileExtension = Path.GetExtension(item);
                        string toFileName = fromFileName + fromFileExtension;
                        string toFilePath = Path.Combine(toPath, toFileName);
                        int fileNamePrefix = 0;
                        while (File.Exists(toFilePath))
                        {
                            toFileName = fromFileName + fileNamePrefix.ToString() + fromFileExtension;
                            toFilePath = Path.Combine(toPath, toFileName);
                            fileNamePrefix++;
                        }
                        File.Copy(fromFilePath, toFilePath, true);
                        collection.RemoveIgnorRules(item);
                        collection.AddIgnorRules(Path.Combine(prefixPath, toFileName), true, null);
                    }
                    collection.ClearIrrelevantItems();
                    collection.OriginalFolderName = collectionName;
                    collection.IsChanged = true;
                    // write collection description
                    if (!string.IsNullOrEmpty(collection.Description))
                    {
                        Dispatcher.Invoke(() => logParagraph.Inlines.Add("Сохранение описания коллекции...\r\n"));
                        string descriptionFile = Path.Combine(toPath, "description.txt");
                        using (StreamWriter descriptionWriter = new StreamWriter(descriptionFile, false, Encoding.UTF8))
                        {
                            descriptionWriter.Write(collection.Description);
                        }
                    }
                }
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Установка параметров...\r\n"));
                CollectionStore.ClearIrrelevantDistribution();
                CollectionStore.ClearIrrelevantSaved();
                CollectionStore.Settings.SetDistributionDirectoryAsBase();
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Удаление директории данных программы...\r\n"));
                if (Directory.Exists(dataDirectory))
                {
                    Directory.Delete(dataDirectory, true);
                }
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Сохранение параметров...\r\n"));
                string newDataDirectory = Path.Combine(distributionDirectory, CollectionStore.DataDirectoryName);
                Directory.CreateDirectory(newDataDirectory);
                CollectionStore.Settings.Save();
                BaseSaveCollectionsTaskAction();
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Распределяет файлы по папкам
        /// </summary>
        private void StdDistributionTaskAction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                });
                List<string[]> collectionRename = new List<string[]>();
                string baseDirectory = CollectionStore.Settings.BaseDirectory;
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                string previewDirectory = Path.Combine(baseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
                foreach (string collectionName in CollectionStore.ActualCollections)
                {
                    Dispatcher.Invoke((Action<string>)((string _collection) => logParagraph.Inlines.Add($"Обработка коллекции: \"{_collection}\"\r\n")), collectionName);
                    Collection collection = CollectionStore.Get(collectionName);
                    // add data to rename list
                    if (!string.IsNullOrEmpty(collection.OriginalFolderName) && !collectionName.Equals(collection.OriginalFolderName))
                    {
                        collectionRename.Add(new string[] { collectionName, collection.OriginalFolderName });
                    }
                    // remove from delete list
                    if (CollectionStore.ContainsIrrelevantDistribution(collectionName))
                    {
                        CollectionStore.RemoveIrrelevantDistribution(collectionName);
                    }
                    // distribution process
                    string prefixPath = string.Empty;
                    string prefixItem = string.Empty;
                    if (collection.Id != CollectionStore.BaseCollectionId)
                    {
                        prefixPath = collectionName;
                        if (!string.IsNullOrEmpty(collection.OriginalFolderName))
                        {
                            prefixPath = collection.OriginalFolderName;
                        }
                        prefixItem = collectionName;
                    }
                    string toPath = Path.Combine(baseDirectory, prefixPath);
                    Directory.CreateDirectory(toPath);
                    List<string> actualItemsTemp = new List<string>(collection.ActualItemsKeys);
                    foreach (string item in actualItemsTemp)
                    {
                        CollectionItemMeta meta = collection[item];
                        if (meta.InCurrentFolder)
                        {
                            continue;
                        }
                        string fromFilePath = Path.Combine(baseDirectory, item);
                        Dispatcher.Invoke((Action<string>)((string _fromFilePath) => logParagraph.Inlines.Add($"Подготовка и копирование: \"{_fromFilePath}\"\r\n")), fromFilePath);
                        string fromFileName = Path.GetFileNameWithoutExtension(item);
                        string fromFileExtension = Path.GetExtension(item);
                        string toFileName = fromFileName + fromFileExtension;
                        string toFilePath = Path.Combine(toPath, toFileName);
                        int fileNamePrefix = 0;
                        while (File.Exists(toFilePath))
                        {
                            toFileName = fromFileName + fileNamePrefix.ToString() + fromFileExtension;
                            toFilePath = Path.Combine(toPath, toFileName);
                            fileNamePrefix++;
                        }
                        File.Move(fromFilePath, toFilePath);
                        string newItem = Path.Combine(prefixItem, toFileName);
                        collection.RemoveIgnorRules(item);
                        collection.AddIgnorRules(newItem, true, null, meta);
                        collection.IsChanged = true;
                        if (!string.IsNullOrEmpty(meta.Hash))
                        {
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Установка параметров элемента...\r\n"));
                            byte[] newHashB = md5.ComputeHash(Encoding.UTF8.GetBytes(newItem));
                            StringBuilder newHashSB = new StringBuilder(newHashB.Length * 2);
                            for (int i = 0; i < newHashB.Length; i++)
                            {
                                newHashSB.Append(newHashB[i].ToString("X2"));
                            }
                            string newHash = newHashSB.ToString();
                            string oldPreview = Path.Combine(previewDirectory, $"{meta.Hash}.jpg");
                            string newPreview = Path.Combine(previewDirectory, $"{newHash}.jpg");
                            /*if (File.Exists(newPreview))
                            {
                                File.Delete(newPreview);
                            }*/
                            File.Move(oldPreview, newPreview);
                            meta.Hash = newHash;
                        }
                    }
                    collection.OriginalFolderName = collectionName;
                    bool isClear = collection.ClearIrrelevantItems();
                    collection.IsChanged = collection.IsChanged || isClear;
                    // write collection description
                    if (!string.IsNullOrEmpty(collection.Description))
                    {
                        Dispatcher.Invoke(() => logParagraph.Inlines.Add("Сохранение описания коллекции...\r\n"));
                        string descriptionFile = Path.Combine(toPath, "description.txt");
                        using (StreamWriter descriptionWriter = new StreamWriter(descriptionFile, false, Encoding.UTF8))
                        {
                            descriptionWriter.Write(collection.Description);
                        }
                    }
                }
                // rename folders
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Подготовка к переименованию папок коллекций...\r\n"));
                foreach (string[] item in collectionRename)
                {
                    string originalNameFrom = Path.Combine(baseDirectory, item[1]);
                    string originalNameTo = Path.Combine(baseDirectory, $"temp-{item[1]}");
                    Directory.Move(originalNameFrom, originalNameTo);
                }
                foreach (string[] item in collectionRename)
                {
                    string originalNameFrom = Path.Combine(baseDirectory, $"temp-{item[1]}");
                    string originalNameTo = Path.Combine(baseDirectory, item[0]);
                    Dispatcher.Invoke((Action<string, string>)((string _fromName, string _toName) => logParagraph.Inlines.Add($"Переименование \"{_fromName}\" в \"{_toName}\"")), item[1], item[0]);
                    Directory.Move(originalNameFrom, originalNameTo);
                    Collection collection = CollectionStore.Get(item[0]);
                    collection.IsChanged = true;
                }
                // deleted irrelevant folder
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Удаление пустых папок коллекций...\r\n"));
                foreach (string removeCollection in CollectionStore.IrrelevantDistributionCollections)
                {
                    string removeCollectionDirPath = Path.Combine(baseDirectory, removeCollection);
                    DirectoryInfo removeCollectionDir = new DirectoryInfo(removeCollectionDirPath);
                    if (removeCollectionDir.Exists)
                    {
                        FileInfo[] allFiles = removeCollectionDir.GetFiles("*", SearchOption.AllDirectories);
                        int count = allFiles.Length;
                        if (count > 1)
                        {
                            continue;
                        }
                        else if (count == 1)
                        {
                            string loverName = allFiles[0].Name.ToLower();
                            if (!loverName.Equals("description.txt"))
                            {
                                continue;
                            }
                        }
                        removeCollectionDir.Delete(true);
                    }
                }
                CollectionStore.ClearIrrelevantDistribution();
                BaseSaveCollectionsTaskAction();
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Переименовывание файлов входящих в коллекцию
        /// </summary>
        /// <param name="collectionName">Название коллекции</param>
        /// <param name="mask">Маска имени файла</param>
        public void RenameAllItemsInCollectionTaskAction(string collectionName, string mask)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Подготовка...\r\n");
                });
                Dictionary<string, int> number = new Dictionary<string, int>() {
                    { ".bmp", 0 },
                    { ".jpg", 0 },
                    { ".jpeg", 0 },
                    { ".png", 0 }
                };
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                Collection collection = CollectionStore.Get(collectionName);
                List<string> actualItemsTemp = new List<string>(collection.ActualItemsKeys);
                string baseDirectory = CollectionStore.Settings.BaseDirectory;
                string previewDirectory = Path.Combine(baseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
                Dispatcher.Invoke((Action<string>)((string _collectionName) => logParagraph.Inlines.Add($"Обработка элементов коллекции \"{_collectionName}\"...\r\n")), collectionName);
                while (actualItemsTemp.Count != 0)
                {
                    string item = actualItemsTemp[0];
                    string extension = Path.GetExtension(item);
                    string newName = string.Format($"{mask}{extension}", number[extension]);
                    number[extension]++;
                    string dirName = Path.GetDirectoryName(item);
                    newName = (string.IsNullOrEmpty(dirName) ? "" : $"{dirName}\\") + newName;
                    if (actualItemsTemp.Contains(newName))
                    {
                        actualItemsTemp.Remove(newName);
                    }
                    else
                    {
                        string fromPath = $"{baseDirectory}\\{item}";
                        string toPath = $"{baseDirectory}\\{newName}";
                        Dispatcher.Invoke((Action<string>)((string _item) => logParagraph.Inlines.Add($"Переименование: \"{_item}\"...\r\n")), item);
                        File.Move(fromPath, toPath);
                        collection.Rename(item, newName);
                        actualItemsTemp.RemoveAt(0);
                        CollectionItemMeta itemMeta = collection[newName];
                        if (!string.IsNullOrEmpty(itemMeta.Hash))
                        {
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Установка параметров элемента...\r\n"));
                            byte[] newHashB = md5.ComputeHash(Encoding.UTF8.GetBytes(newName));
                            StringBuilder newHashSB = new StringBuilder(newHashB.Length * 2);
                            for (int i = 0; i < newHashB.Length; i++)
                            {
                                newHashSB.Append(newHashB[i].ToString("X2"));
                            }
                            string newHash = newHashSB.ToString();
                            string oldPreview = Path.Combine(previewDirectory, $"{itemMeta.Hash}.jpg");
                            string newPreview = Path.Combine(previewDirectory, $"{newHash}.jpg");
                            /*if (File.Exists(newPreview))
                            {
                                File.Delete(newPreview);
                            }*/
                            File.Move(oldPreview, newPreview);
                            itemMeta.Hash = newHash;
                        }
                    }
                }
                foreach (KeyValuePair<string, int> numberItem in number)
                {
                    if (numberItem.Value > 0)
                    {
                        collection.IsChanged = true;
                        break;
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }

        /// <summary>
        /// Объединение коллекций
        /// </summary>
        /// <param name="mergeCollectionPath">Путь к коллекциям для объединения с текущими</param>
        public void MergeCollectionsTaskAction(string mergeCollectionPath)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Поиск метаданных коллекций...\r\n");
                });
                string metaDirectory = Path.Combine(mergeCollectionPath, CollectionStore.DataDirectoryName);
                IEnumerable<string> icdFiles = new DirectoryInfo(metaDirectory)
                    .EnumerateFiles()
                    .Where(x => x.Extension.Equals(".icd"))
                    .Select(x => x.Name);
                foreach (string icdFileName in icdFiles)
                {
                    string icdFilePath = Path.Combine(metaDirectory, icdFileName);
                    Dispatcher.Invoke((Action<string>)((string _icdFilePath) => logParagraph.Inlines.Add($"Обработка: \"{_icdFilePath}\"\r\n")), icdFilePath);
                    Collection collection = null;
                    string collectionName = null;
                    using (FileStream icdFile = new FileStream(icdFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader icdReader = new BinaryReader(icdFile, Encoding.UTF8))
                        {
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Чтение базовых сведений...\r\n"));
                            Guid id = new Guid(icdReader.ReadBytes(16));
                            collection = new Collection(id);
                            collectionName = icdReader.ReadString();
                            if (icdReader.ReadBoolean())
                            {
                                collection.Description = icdReader.ReadString();
                            }
                            bool ofnContains = icdReader.ReadBoolean();
                            if (ofnContains || id == CollectionStore.BaseCollectionId)
                            {
                                if (ofnContains)
                                {
                                    collection.OriginalFolderName = icdReader.ReadString();
                                }
                                // get files from folder and add coll
                                string collectionFolder = mergeCollectionPath;
                                if (id != CollectionStore.BaseCollectionId)
                                {
                                    collectionFolder = Path.Combine(mergeCollectionPath, collection.OriginalFolderName);
                                }
                                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Получение файлов коллекции...\r\n"));
                                IEnumerable<string> files = new DirectoryInfo(collectionFolder)
                                    .EnumerateFiles()
                                    .Where(x => x.Extension.Equals(".bmp") || x.Extension.Equals(".jpg") || x.Extension.Equals(".jpeg") || x.Extension.Equals(".png"))
                                    .Select(x => x.FullName);
                                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Добавление файлов...\r\n"));
                                foreach (string filePath in files)
                                {
                                    collection.AddIgnorRules(filePath.Remove(0, mergeCollectionPath.Length + 1), true, null);
                                }
                            }
                            // read icd processing
                            Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка актуальных и исключенных элементов...\r\n"));
                            while (icdFile.Length != icdFile.Position)
                            {
                                // add
                                if (icdReader.ReadBoolean())
                                {
                                    Guid? parentId = null;
                                    // contains guid
                                    if (icdReader.ReadBoolean())
                                    {
                                        parentId = new Guid(icdReader.ReadBytes(16));
                                    }
                                    string item = icdReader.ReadString();
                                    collection.AddIgnorRules(item, false, parentId);
                                }
                                // remove
                                else
                                {
                                    string item = icdReader.ReadString();
                                    collection.Remove(item);
                                }
                            }
                        }
                    }
                    // merge
                    Dispatcher.Invoke((Action<string>)((string _mergeCollection) => logParagraph.Inlines.Add($"Слияние коллекции \"{_mergeCollection}\"...")), collectionName);
                    if (!CollectionStore.Contains(collectionName))
                    {
                        CollectionStore.Add(new Structures.CollectionInformation(collectionName, false, null, false));
                    }
                    Collection currentCollection = CollectionStore.Get(collectionName);
                    string toCollectionPath = CollectionStore.Settings.BaseDirectory;
                    bool hasDirectory = false;
                    Guid? parentId = CollectionStore.BaseCollectionId;
                    if (!string.IsNullOrEmpty(currentCollection.OriginalFolderName))
                    {
                        toCollectionPath = Path.Combine(CollectionStore.Settings.BaseDirectory, currentCollection.OriginalFolderName);
                        hasDirectory = true;
                    }
                    else if (currentCollection.Id.Equals(CollectionStore.BaseCollectionId))
                    {
                        hasDirectory = true;
                        parentId = null;
                    }
                    foreach (KeyValuePair<string, CollectionItemMeta> item in collection.ActualItems)
                    {
                        string fromFilePath = Path.Combine(mergeCollectionPath, item.Key);
                        string fileName = Path.GetFileName(fromFilePath);
                        string toFilePath = Path.Combine(toCollectionPath, fileName);
                        int counter = 0;
                        while (File.Exists(toFilePath))
                        {
                            toFilePath = Path.Combine(toCollectionPath, $"{counter}{fileName}");
                            counter++;
                        }
                        File.Move(fromFilePath, toFilePath);
                    }
                }
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Сохранение изменений...\r\n"));
                BaseSaveCollectionsTaskAction();
                Dispatcher.Invoke(() =>
                {
                    inProgress = false;
                    Close();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action<string>)((valueError) =>
                {
                    inProgress = false;
                    Run run = new Run(valueError)
                    {
                        Foreground = Brushes.Red,
                    };
                    logParagraph.Inlines.Add(run);
                }), ex.Message);
            }
        }
        #endregion
    }
}