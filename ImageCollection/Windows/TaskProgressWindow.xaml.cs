using ImageCollection.Enums;
using ImageCollection.Classes.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Security.Cryptography;

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
            if (!Directory.Exists(metaDirectory))
            {
                Directory.CreateDirectory(metaDirectory);
            }
            else
            {
                Dispatcher.Invoke(() => logParagraph.Inlines.Add($"Обработка удаленных коллекций...\r\n"));
                string dicdFilePath = Path.Combine(metaDirectory, $"irrelevant.dicd");
                using (FileStream dicdFile = new FileStream(dicdFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter dicdWriter = new BinaryWriter(dicdFile, Encoding.UTF8))
                    {
                        foreach (KeyValuePair<string, Guid?> deleteCollection in CollectionStore.IrrelevantCollections)
                        {
                            Dispatcher.Invoke((Action<string>)((string _collectionName) => logParagraph.Inlines.Add($"Запись названия: \"{_collectionName}\"\r\n")), deleteCollection.Key);
                            dicdWriter.Write(deleteCollection.Key);
                            if (deleteCollection.Value.HasValue)
                            {
                                string deleteIcdFilePath = Path.Combine(metaDirectory, $"{deleteCollection.Value.Value}.icd");
                                if (File.Exists(deleteIcdFilePath))
                                {
                                    Dispatcher.Invoke((Action<string>)((string _deleteFilePath) => logParagraph.Inlines.Add($"Удаление: \"{_deleteFilePath}\"\r\n")), deleteIcdFilePath);
                                    File.Delete(deleteIcdFilePath);
                                }
                            }
                        }
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
                string metaDirectory = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName);
                IEnumerable<string> icdFiles = new DirectoryInfo(metaDirectory)
                    .EnumerateFiles()
                    .Where(x => x.Extension.Equals(".icd"))
                    .Select(x => Name);
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
                string dicdFilePath = Path.Combine(metaDirectory, $"irrelevant.dicd");
                if (File.Exists(dicdFilePath))
                {
                    using (FileStream dicdFile = new FileStream(dicdFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader dicdReader = new BinaryReader(dicdFile, Encoding.UTF8))
                        {
                            while (dicdFile.Length != dicdFile.Position)
                            {
                                string deleteCollectionName = dicdReader.ReadString();
                                CollectionStore.AddIrrelevant(deleteCollectionName);
                            }
                        }
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
                    collection.IsChanged = true;
                }
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Установка параметров...\r\n"));
                CollectionStore.Settings.SetDistributionDirectoryAsBase();
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Сохранение параметров...\r\n"));
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

        private void StdDistributionTaskAction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                });
                IEnumerable<string> collectionNames = CollectionStore.GetCollectionNames();
                foreach (string collectionName in collectionNames)
                {
                    Dispatcher.Invoke((Action<string>)((string paramCollectionName) => logParagraph.Inlines.Add($"Обработка коллекции \"{paramCollectionName}\"...\r\n")), collectionName);
                    string toPrefix = $"{collectionName}\\";
                    if (collectionName.Equals(CollectionStore.BaseCollectionName))
                        toPrefix = "";
                    else
                        Directory.CreateDirectory($"{CollectionStore.BaseDirectory}\\{collectionName}");
                    Collection collection = CollectionStore.Get(collectionName);
                    List<string> actualItemsTemp = new List<string>(collection.ActualItems);
                    MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                    string toPath;
                    string fromPath;
                    string fromFileName;
                    string toFileName;
                    byte[] oldPreviewNameB;
                    byte[] newPreviewNameB;
                    StringBuilder oldPreviewNameS;
                    StringBuilder newPreviewNameS;
                    foreach (string item in actualItemsTemp)
                    {
                        CollectionItemMeta itemMeta = collection[item];
                        if (itemMeta.InCurrentFolder)
                            continue;
                        fromPath = $"{CollectionStore.BaseDirectory}\\{item}";
                        fromFileName = Path.GetFileName(item);
                        toFileName = fromFileName;
                        toPath = $"{CollectionStore.BaseDirectory}\\{toPrefix}{toFileName}";
                        int index = 0;
                        while (File.Exists(toPath))
                        {
                            toFileName = index + fromFileName;
                            toPath = $"{CollectionStore.BaseDirectory}\\{toPrefix}{toFileName}";
                            index++;
                        }

                        Dispatcher.Invoke((Action<string, string>)((string paramFromPath, string paramToPath) => logParagraph.Inlines.Add($"Перемещение \"{paramFromPath}\" -> \"{paramToPath}\"...\r\n")), fromPath, toPath);
                        File.Move(fromPath, toPath);
                        try
                        {
                            oldPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(item));
                            newPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(toPrefix + toFileName));
                            oldPreviewNameS = new StringBuilder(oldPreviewNameB.Length * 2);
                            newPreviewNameS = new StringBuilder(newPreviewNameB.Length * 2);
                            for (int i = 0; i < oldPreviewNameB.Length; i++)
                            {
                                oldPreviewNameS.Append(oldPreviewNameB[i].ToString("X2"));
                                newPreviewNameS.Append(newPreviewNameB[i].ToString("X2"));
                            }
                            File.Move($"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{oldPreviewNameS}.jpg",
                                $"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{newPreviewNameS}.jpg");
                        }
                        catch { }
                        collection.RemoveIgnorRules(item);
                        collection.AddIgnorRules(toPrefix + toFileName, true, null);
                        collection.IsChanged = true;
                    }
                    if (!string.IsNullOrEmpty(collection.Description))
                    {
                        Dispatcher.Invoke(() => logParagraph.Inlines.Add("Запись описания коллекции...\r\n"));
                        File.WriteAllText($"{CollectionStore.BaseDirectory}\\{toPrefix}description.txt", collection.Description, Encoding.UTF8);
                    }
                    if (collection.ClearIrrelevantItems())
                        collection.IsChanged = true;
                }
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка удаленных коллекции...\r\n"));
                foreach (string collectionName in CollectionStore.IrrelevantCollections)
                {
                    string path = $"{CollectionStore.BaseDirectory}\\{collectionName}";
                    if (Directory.Exists(path))
                    {
                        Dispatcher.Invoke((Action<string>)((string paramPath) => logParagraph.Inlines.Add($"Удаление \"{paramPath}\"...\r\n")), path);
                        Directory.Delete(path, true);
                    }
                }
                MessageBoxResult messageBoxResult = Dispatcher.Invoke(() => MessageBox.Show("Сохранить конфигурацию коллекций сейчас?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question));
                if (messageBoxResult == MessageBoxResult.Yes)
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
                Dictionary<string, int> extensions = new Dictionary<string, int>() {
                    { ".bmp", 0 }, { ".jpg", 0 }, { ".jpeg", 0 }, { ".png", 0 }
                };
                Collection collection = CollectionStore.Get(collectionName);
                List<string> actualItems = new List<string>(collection.ActualItems);
                MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                byte[] oldPreviewNameB;
                byte[] newPreviewNameB;
                StringBuilder oldPreviewNameS;
                StringBuilder newPreviewNameS;
                while (actualItems.Count != 0)
                {
                    string item = actualItems[0];
                    string fromPath = $"{CollectionStore.BaseDirectory}\\{item}";
                    Dispatcher.Invoke((Action<string>)((string paramFrom) => logParagraph.Inlines.Add($"Обработка \"{paramFrom}\"...\r\n")), fromPath);
                    string extension = Path.GetExtension(item);
                    string newName = string.Format($"{mask}{extension}", extensions[extension]);
                    string dirName = Path.GetDirectoryName(item);
                    newName = (string.IsNullOrEmpty(dirName) ? "" : $"{dirName}\\") + newName;
                    extensions[extension]++;
                    if (actualItems.Contains(newName))
                        actualItems.Remove(newName);
                    else
                    {
                        string toPath = $"{CollectionStore.BaseDirectory}\\{newName}";
                        Dispatcher.Invoke((Action<string, string>)((string paramFrom, string paramTo) => logParagraph.Inlines.Add($"Переименование: \"{paramFrom}\" -> \"{paramTo}\"...\r\n")), fromPath, toPath);
                        File.Move(fromPath, toPath);
                        try
                        {
                            oldPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(item));
                            newPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(newName));
                            oldPreviewNameS = new StringBuilder(oldPreviewNameB.Length * 2);
                            newPreviewNameS = new StringBuilder(newPreviewNameB.Length * 2);
                            for (int i = 0; i < oldPreviewNameB.Length; i++)
                            {
                                oldPreviewNameS.Append(oldPreviewNameB[i].ToString("X2"));
                                newPreviewNameS.Append(newPreviewNameB[i].ToString("X2"));
                            }
                            File.Move($"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{oldPreviewNameS}.jpg",
                                $"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{newPreviewNameS}.jpg");
                        }
                        catch { }
                        collection.Rename(item, newName);
                        collection.IsChanged = true;
                        actualItems.RemoveAt(0);
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

        private void OpenFolderTaskAction(string baseDirectory, SearchOption recursiveSearch, string searchMask, string distributionDirectory)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    inProgress = true;
                    progressBar_Progress.IsIndeterminate = true;
                    logParagraph.Inlines.Add("Получение списка файлов...\r\n");
                });
                DirectoryInfo directoryInfo = new DirectoryInfo(baseDirectory);
                IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles(searchMask, recursiveSearch)
                    .Where(f => f.Extension.Equals(".bmp") || f.Extension.Equals(".jpg") || f.Extension.Equals(".jpeg") || f.Extension.Equals(".png"));
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Инициализация хранилища...\r\n"));
                CollectionStore.Init(baseDirectory, distributionDirectory);
                Collection collection = new Collection(Guid.Parse(CollectionStore.BaseCollectionGuid));
                Dispatcher.Invoke(() => logParagraph.Inlines.Add("Обработка файлов...\r\n"));
                foreach (FileInfo file in files)
                {
                    string path = file.DirectoryName;
                    if (path != baseDirectory)
                        collection.AddIgnorRules(file.FullName.Remove(0, baseDirectory.Length + 1), false, null);
                    else
                        collection.AddIgnorRules(file.Name, true, null);
                }
                collection.IsChanged = true;
                CollectionStore.Add(CollectionStore.BaseCollectionName, collection);
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