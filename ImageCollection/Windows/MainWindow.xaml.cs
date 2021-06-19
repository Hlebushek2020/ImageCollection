using ImageCollection.Classes.Collections;
using ImageCollection.Classes.ItemMovers;
using ImageCollection.Classes.Settings;
using ImageCollection.Classes.Views;
using ImageCollection.Enums;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ListBoxImageItem> collectionItems = new ObservableCollection<ListBoxImageItem>();

        private volatile bool stopImageTask;
        private Task imageTask;

        public MainWindow()
        {
            InitializeComponent();
            Title = App.Name;

            comboBox_CollectionNames.ItemsSource = CollectionStore.ActualCollections;
            listBox_CollectionItems.ItemsSource = collectionItems;

            ProgramSettings settings = ProgramSettings.GetInstance();
            if (settings.Theme.ToLower().Equals("light"))
            {
                menuItem_Light.IsChecked = true;
            }
            else
            {
                menuItem_Dark.IsChecked = true;
            }
        }

        private void RefreshAfterOpening()
        {
            object currentColltctionName = comboBox_CollectionNames.SelectedItem;
            comboBox_CollectionNames.Items.Refresh();
            comboBox_CollectionNames.SelectedItem = CollectionStore.BaseCollectionName;
            if (currentColltctionName != null)
                ComboBox_CollectionNames_SelectionChanged(null, null);
        }

        private void MenuItem_OpenFolder_Click(object sender, RoutedEventArgs e) =>
            OpenFolderShell();

        private void OpenFolderShell(string folder = null)
        {
            SettingsOpenFolderWindow settingsOpenFolderWindow = new SettingsOpenFolderWindow(folder);
            settingsOpenFolderWindow.ShowDialog();
            OpenFolderArgs openFolderArgs = settingsOpenFolderWindow.GetArgs();
            if (openFolderArgs.ContinueExecution)
            {
                TaskProgressWindow progressWindow = new TaskProgressWindow(TaskType.OpenFolder, new object[] {
                    openFolderArgs.BaseDirectory, openFolderArgs.SearchOption, openFolderArgs.SearchMask, openFolderArgs.DistributionDirectory
                });
                progressWindow.ShowDialog();
                RefreshAfterOpening();
            }
        }

        private void MenuItem_OpenCollections_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string baseDirectory = folderBrowserDialog.SelectedPath;
                    if (Directory.Exists($"{baseDirectory}\\{CollectionStore.DataDirectoryName}"))
                    {
                        TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.OpenCollections, new object[] { baseDirectory });
                        taskProgressWindow.ShowDialog();
                        RefreshAfterOpening();
                    }
                    else
                        MessageBox.Show("Папка, содержащая данные о коллекциях не обнаружена. Продолжение операции невозможно.",
                            App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

        }

        private void MenuItem_SaveCollections_Click(object sender, RoutedEventArgs e)
        {
            TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.SaveCollections);
            taskProgressWindow.ShowDialog();
        }

        private void MenuItem_RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null && listBox_CollectionItems.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                ListBoxImageItem currentCollectionItem = (ListBoxImageItem)listBox_CollectionItems.SelectedItem;
                int currentCollectionItemIndex = listBox_CollectionItems.SelectedIndex;
                string deleteFile = Path.Combine(CollectionStore.Settings.BaseDirectory, currentCollectionItem.Path);
                if (MessageBox.Show($"Удалить \"{deleteFile}\"?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Collection collection = CollectionStore.Get(currentCollectionName);
                    File.Delete(deleteFile);
                    collection.RemoveIgnorRules(currentCollectionItem.Path);
                    collection.IsChanged = true;
                    collectionItems.RemoveAt(currentCollectionItemIndex);

                    listBox_CollectionItems.SelectedIndex = Math.Min(currentCollectionItemIndex, collectionItems.Count - 1);

                    try
                    {
                        if (!string.IsNullOrEmpty(currentCollectionItem.Hash))
                        {
                            string deletePreviewFile = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName,
                            CollectionStore.PreviewDirectoryName, $"{currentCollectionItem.Hash}.jpg");
                            if (File.Exists(deletePreviewFile))
                            {
                                File.Delete(deletePreviewFile);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private void MenuItem_ToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null && listBox_CollectionItems.SelectedItems.Count > 0)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                SelectCollectionWindow selectCollectionWindow = new SelectCollectionWindow(currentCollectionName);
                selectCollectionWindow.ShowDialog();
                if (selectCollectionWindow.IsApply)
                {
                    string toCollectionName = selectCollectionWindow.GetNameSelectedCollection();
                    IEnumerable<ListBoxImageItem> enSelectedItems = listBox_CollectionItems.SelectedItems.Cast<ListBoxImageItem>();

                    int firstCollectionItemSelectedIndex = enSelectedItems.Min(x => collectionItems.IndexOf(x));

                    List<ListBoxImageItem> selectedItems = new List<ListBoxImageItem>(enSelectedItems);
                    ItemMover itemMover = CollectionStore.InitializeItemMover(currentCollectionName, toCollectionName);
                    foreach (ListBoxImageItem item in selectedItems)
                    {
                        collectionItems.Remove(item);
                        itemMover.Move(item.Path);
                    }
                    itemMover.EndMoving();

                    listBox_CollectionItems.SelectedIndex = Math.Min(firstCollectionItemSelectedIndex, listBox_CollectionItems.Items.Count - 1);
                }
            }
        }

        private void ComboBox_CollectionNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageTask != null)
            {
                if (imageTask.Status == TaskStatus.Running)
                {
                    stopImageTask = true;
                    imageTask.Wait();
                }
            }
            collectionItems.Clear();
            if (comboBox_CollectionNames.SelectedItem != null)
            {
                string collectionName = (string)comboBox_CollectionNames.SelectedItem;
                imageTask = Task.Run(() => CreateItemList(collectionName));
            }
        }

        #region Construct Collection Item List
        private void CreateItemList(string collectionName)
        {
            // preparation
            stopImageTask = false;
            string previewFolder = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
            if (!Directory.Exists(previewFolder))
            {
                Directory.CreateDirectory(previewFolder);
            }
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            // processing
            Collection collection = CollectionStore.Get(collectionName);
            foreach (KeyValuePair<string, CollectionItemMeta> collectionItem in collection.ActualItems)
            {
                if (stopImageTask)
                {
                    break;
                }
                ListBoxImageItem listBoxImageItem = new ListBoxImageItem(collectionItem.Key, collectionItem.Value);
                GeneratePreviewAndDescription(previewFolder, collectionItem.Key, md5, listBoxImageItem);
                if (stopImageTask)
                {
                    break;
                }
                Dispatcher.Invoke(() =>
                {
                    string previewFile = Path.Combine(previewFolder, $"{listBoxImageItem.Hash}.jpg");
                    MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(previewFile));
                    BitmapImage preview = new BitmapImage();
                    preview.BeginInit();
                    preview.StreamSource = memoryStream;
                    preview.EndInit();
                    listBoxImageItem.Preview = preview;
                    collectionItems.Add(listBoxImageItem);
                });
            }
        }

        private void GeneratePreviewAndDescription(string previewFolder, string item, MD5CryptoServiceProvider md5, ListBoxImageItem imageItem)
        {
            if (string.IsNullOrEmpty(imageItem.Hash))
            {
                // processing generate hash
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(item));
                StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    stringBuilder.Append(hash[i].ToString("X2"));
                }
                imageItem.Hash = stringBuilder.ToString();
            }
            string previewImage = Path.Combine(previewFolder, $"{imageItem.Hash}.jpg");
            if (string.IsNullOrEmpty(imageItem.Description) || !File.Exists(previewImage))
            {
                // preparation
                string originalImage = Path.Combine(CollectionStore.Settings.BaseDirectory, item);
                BitmapImage bitmapImage = new BitmapImage();
                byte[] originalImageBuffer = File.ReadAllBytes(originalImage);
                // processing description
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(originalImageBuffer);
                bitmapImage.EndInit();
                imageItem.Description = $"{bitmapImage.PixelWidth}x{bitmapImage.PixelHeight}; {Math.Round(originalImageBuffer.Length / 1024.0 / 1024.0, 2)} Мб";
                // processing generate preview
                if (!File.Exists(previewImage))
                {
                    int decodeWidth = (int)(1.0 * bitmapImage.PixelWidth / bitmapImage.PixelHeight * 94.0);
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(originalImageBuffer);
                    bitmapImage.DecodePixelHeight = 94;
                    bitmapImage.DecodePixelWidth = decodeWidth;
                    bitmapImage.EndInit();
                    JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
                    jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (FileStream fileStream = new FileStream(previewImage, FileMode.Create, FileAccess.Write))
                    {
                        jpegBitmapEncoder.Save(fileStream);
                    }
                }
            }
        }
        #endregion

        private void ListBox_CollectionItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_CollectionItems.SelectedItem != null)
            {
                string currentCollectionItem = ((ListBoxImageItem)listBox_CollectionItems.SelectedItem).Path;
                string pathImage = $"{CollectionStore.Settings.BaseDirectory}\\{currentCollectionItem}";
                byte[] buffer = File.ReadAllBytes(pathImage);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(buffer);
                bitmapImage.EndInit();
                image_Image.Source = bitmapImage;
                double actualWidth = ((Grid)image_Image.Parent).ActualWidth;
                double actualHeight = ((Grid)image_Image.Parent).ActualHeight;
                if (bitmapImage.PixelWidth < actualWidth && bitmapImage.PixelHeight < actualHeight)
                {
                    image_Image.Height = bitmapImage.PixelHeight;
                    image_Image.Width = bitmapImage.PixelWidth;
                    image_Image.Stretch = Stretch.Fill;
                }
                else
                {
                    image_Image.Height = double.NaN;
                    image_Image.Width = double.NaN;
                    image_Image.Stretch = Stretch.Uniform;
                }
                textBlock_Intelligence.Text = $"{bitmapImage.PixelWidth}x{bitmapImage.PixelHeight}; {Math.Round(buffer.Length / 1024.0 / 1024.0, 2)} Мб;";
            }
            else
            {
                image_Image.Source = null;
                textBlock_Intelligence.Text = string.Empty;
            }
        }

        private void MenuItem_CreateCollection_Click(object sender, RoutedEventArgs e)
        {
            CollectionInformationEditorWindow collectionInformationEditor = new CollectionInformationEditorWindow();
            collectionInformationEditor.ShowDialog();
            CollectionInformation collectionInformation = collectionInformationEditor.GetCollectionInformation();
            if (collectionInformation.ChangedName)
            {
                CollectionStore.Add(collectionInformation);
                comboBox_CollectionNames.Items.Refresh();
            }
        }

        private void MenuItem_EditCollectionDetails_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                Collection collection = CollectionStore.Get(currentCollectionName);
                CollectionInformationEditorWindow collectionInformationEditor = new CollectionInformationEditorWindow(currentCollectionName, collection.Description);
                collectionInformationEditor.ShowDialog();
                CollectionInformation collectionInformation = collectionInformationEditor.GetCollectionInformation();
                CollectionStore.Edit(currentCollectionName, collectionInformation);
                if (collectionInformation.ChangedName)
                {
                    comboBox_CollectionNames.Items.Refresh();
                    comboBox_CollectionNames.SelectedItem = collectionInformation.Name;
                }
            }
        }

        private void MenuItem_RemoveCollection_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                if (currentCollectionName.Equals(CollectionStore.BaseCollectionName))
                    MessageBox.Show("Коллекцию по умолчанию запрещено удалять!", App.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                else
                {
                    if (MessageBox.Show($"Удалить коллекцию \"{currentCollectionName}\"?", App.Name,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        CollectionStore.Remove(currentCollectionName);
                        comboBox_CollectionNames.Items.Refresh();
                        comboBox_CollectionNames.SelectedItem = CollectionStore.BaseCollectionName;
                    }
                }
            }
        }

        private void MenuItem_DistributeFolders_Click(object sender, RoutedEventArgs e)
        {
            TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.Distribution);
            taskProgressWindow.ShowDialog();
            ComboBox_CollectionNames_SelectionChanged(null, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (!Directory.Exists(args[1]))
                    Close();

                if (args.Contains("-oc"))
                {
                    if (Directory.Exists($"{args[1]}\\{CollectionStore.DataDirectoryName}"))
                    {
                        TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.OpenCollections, new object[] { args[1] });
                        taskProgressWindow.ShowDialog();
                        RefreshAfterOpening();
                    }
                    else
                    {
                        MessageBox.Show("Папка, содержащая данные о коллекциях не обнаружена. Продолжение операции невозможно.",
                            App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);

                        Close();
                    }
                }
                else
                    OpenFolderShell(args[1]);
            }
            else
            {
                StartWindow startWindow = new StartWindow();
                startWindow.ShowDialog();
                switch (startWindow.StartWork)
                {
                    case StartWork.OpenFolder:
                        MenuItem_OpenFolder_Click(null, null);
                        break;
                    case StartWork.OpenCollection:
                        MenuItem_OpenCollections_Click(null, null);
                        break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string collectionName in CollectionStore.ActualCollections)
            {
                if (CollectionStore.Get(collectionName).IsChanged)
                {
                    if (MessageBox.Show("Текущие изменения не сохранены, закрыть?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        e.Cancel = true;
                    break;
                }
            }
            if (imageTask != null)
            {
                if (imageTask.Status == TaskStatus.Running)
                {
                    stopImageTask = true;
                    imageTask.Wait();
                }
            }
        }

        private void MenuItem_RenameFile_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null && listBox_CollectionItems.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                ListBoxImageItem listBoxImageItem = (ListBoxImageItem)listBox_CollectionItems.SelectedItem;
                RenameFileWindow renameFileWindow = new RenameFileWindow(currentCollectionName, listBoxImageItem.Path);
                renameFileWindow.ShowDialog();
                if (renameFileWindow.IsApply)
                    listBoxImageItem.Path = renameFileWindow.NewFileName;
            }
        }

        private void MenuItem_RenameAllItemsInCollection_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                RenameFileWindow renameFileWindow = new RenameFileWindow(currentCollectionName);
                renameFileWindow.ShowDialog();
                if (renameFileWindow.IsApply)
                    ComboBox_CollectionNames_SelectionChanged(null, null);
            }
        }

        private void MenuItem_RemoveAllSelectedFiles_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_CollectionItems.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Вы действительно хотите удалить выбранные файлы?", App.Name,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                    Collection collection = CollectionStore.Get(currentCollectionName);
                    IEnumerable<ListBoxImageItem> enSelectedItems = listBox_CollectionItems.SelectedItems.Cast<ListBoxImageItem>();

                    int firstCollectionItemSelectedIndex = enSelectedItems.Min(x => collectionItems.IndexOf(x));

                    List<ListBoxImageItem> selectedItems = new List<ListBoxImageItem>(enSelectedItems);
                    foreach (ListBoxImageItem item in selectedItems)
                    {
                        File.Delete(Path.Combine(CollectionStore.Settings.BaseDirectory, item.Path));
                        collection.RemoveIgnorRules(item.Path);
                        collection.IsChanged = true;
                        collectionItems.Remove(item);
                    }

                    listBox_CollectionItems.SelectedIndex = Math.Min(firstCollectionItemSelectedIndex, collectionItems.Count - 1);

                    Task.Run(() =>
                    {
                        string previewFolder = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
                        foreach (ListBoxImageItem item in selectedItems)
                        {
                            if (!string.IsNullOrEmpty(item.Hash))
                            {
                                string deletePreviewFile = Path.Combine(previewFolder, $"{item.Hash}.jpg");
                                if (File.Exists(deletePreviewFile))
                                {
                                    try
                                    {
                                        File.Delete(deletePreviewFile);
                                    }
                                    catch { }
                                }
                            }
                        }
                    });
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        MenuItem_OpenFolder_Click(null, null);
                        break;
                    case Key.S:
                        MenuItem_SaveCollections_Click(null, null);
                        break;
                    case Key.C:
                        MenuItem_ToCollection_Click(null, null);
                        break;
                    case Key.N:
                        MenuItem_CreateCollection_Click(null, null);
                        break;
                    case Key.E:
                        MenuItem_EditCollectionDetails_Click(null, null);
                        break;
                    case Key.D:
                        MenuItem_DistributeFolders_Click(null, null);
                        break;
                    case Key.Delete:
                        MenuItem_RemoveAllSelectedFiles_Click(null, null);
                        break;
                    case Key.F2:
                        MenuItem_RenameAllItemsInCollection_Click(null, null);
                        break;
                }
            }
            else
            {
                if (e.Key == Key.Delete)
                    MenuItem_RemoveFile_Click(null, null);
                if (e.Key == Key.F2)
                    MenuItem_RenameFile_Click(null, null);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (image_Image.Source != null && image_Image.Width != 0)
            {
                double actualWidth = ((Grid)image_Image.Parent).ActualWidth;
                double actualHeight = ((Grid)image_Image.Parent).ActualHeight;
                double pixelWidth = ((BitmapImage)image_Image.Source).PixelWidth;
                double pixelHeight = ((BitmapImage)image_Image.Source).PixelHeight;
                if (pixelWidth < actualWidth && pixelHeight < actualHeight)
                {
                    image_Image.Height = pixelHeight;
                    image_Image.Width = pixelWidth;
                    image_Image.Stretch = Stretch.Fill;
                    return;
                }
                image_Image.Height = double.NaN;
                image_Image.Width = double.NaN;
                image_Image.Stretch = Stretch.Uniform;
            }
        }

        private void MenuItem_СlearImageCache_Click(object sender, RoutedEventArgs e)
        {
            stopImageTask = true;
            imageTask.Wait();
            TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.СlearImageCache);
            taskProgressWindow.ShowDialog();
        }

        private void ListBox_CollectionItems_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (e.Key == Key.Up)
                    if (listBox_CollectionItems.SelectedIndex > 0)
                        listBox_CollectionItems.Items.MoveCurrentToPrevious();
                if (e.Key == Key.Down)
                    if (listBox_CollectionItems.SelectedIndex != listBox_CollectionItems.Items.Count - 1)
                        listBox_CollectionItems.Items.MoveCurrentToNext();
                listBox_CollectionItems.ScrollIntoView(listBox_CollectionItems.SelectedItem);
            }
            e.Handled = true;
        }

        private void MenuItem_SetTheme_Click(object sender, RoutedEventArgs e)
        {
            menuItem_Dark.IsChecked = false;
            menuItem_Light.IsChecked = false;
            MenuItem current = (MenuItem)sender;
            current.IsChecked = true;
            string theme = current.Tag.ToString();
            ProgramSettings settings = ProgramSettings.GetInstance();
            if (!settings.Theme.Equals(theme))
            {
                Uri uri = new Uri($"Themes/{theme}.xaml", UriKind.Relative);
                ResourceDictionary resource = (ResourceDictionary)Application.LoadComponent(uri);
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resource);
                settings.Theme = theme;
                settings.Save();
            }
        }
    }
}