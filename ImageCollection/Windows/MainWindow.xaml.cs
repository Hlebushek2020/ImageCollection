using ImageCollection.Classes;
using ImageCollection.Classes.Static;
using ImageCollection.Enums;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = App.Name;
            comboBox_CollectionNames.ItemsSource = CollectionStore.GetCollectionNames();
        }

        private void RefreshAfterOpening()
        {
            object currentColltctionName = comboBox_CollectionNames.SelectedItem;
            comboBox_CollectionNames.ItemsSource = CollectionStore.GetCollectionNames();
            comboBox_CollectionNames.SelectedItem = CollectionStore.BaseCollectionName;
            if (currentColltctionName != null)
                //if (((string)currentColltctionName).Equals(CollectionStore.BaseCollectionName))
                ComboBox_CollectionNames_SelectionChanged(null, null);
        }

        private void MenuItem_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            SettingsOpenFolderWindow settingsOpenFolderWindow = new SettingsOpenFolderWindow();
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
                string currentCollectionItem = (string)listBox_CollectionItems.SelectedItem;
                if (MessageBox.Show($"Удалить \"{CollectionStore.BaseDirectory}\\{currentCollectionItem}\"?",
                    App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Collection collection = CollectionStore.Get(currentCollectionName);
                    File.Delete($"{CollectionStore.BaseDirectory}\\{currentCollectionItem}");
                    collection.RemovePermanently(currentCollectionItem);
                    listBox_CollectionItems.Items.Refresh();
                }
            }
        }

        private void MenuItem_ToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null && listBox_CollectionItems.SelectedItems.Count > 0)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                IEnumerable<string> currentCollectionItems = listBox_CollectionItems.SelectedItems.Cast<string>();
                SelectCollectionWindow selectCollectionWindow = new SelectCollectionWindow(currentCollectionName);
                selectCollectionWindow.ShowDialog();
                if (selectCollectionWindow.IsApply)
                {
                    string toCollectionName = selectCollectionWindow.GetNameSelectedCollection();
                    CollectionStore.ToCollection(currentCollectionName, toCollectionName, currentCollectionItems);
                    listBox_CollectionItems.Items.Refresh();
                }
            }
        }

        private void ComboBox_CollectionNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                listBox_CollectionItems.ItemsSource = CollectionStore.Get(currentCollectionName).ActualItems;
            }
            else
                listBox_CollectionItems.ItemsSource = null;
        }

        private void ListBox_CollectionItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_CollectionItems.SelectedItem != null)
            {
                string currentCollectionItem = (string)listBox_CollectionItems.SelectedItem;
                string pathImage = $"{CollectionStore.BaseDirectory}\\{currentCollectionItem}";
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
                textBlock_Intelligence.Text = $"{bitmapImage.PixelWidth}x{bitmapImage.PixelHeight}; {Math.Round(buffer.Length / 1024.0 / 1024.0, 2)} Мб";
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
                CollectionStore.Add(collectionInformation.Name, collectionInformation.Description);
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
                if (collectionInformation.ChangedDescription)
                    collection.Description = collectionInformation.Description;
                if (collectionInformation.ChangedName)
                {
                    CollectionStore.Rename(currentCollectionName, collectionInformation.Name);
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
                    }
                }
            }
        }

        private void MenuItem_DistributeFolders_Click(object sender, RoutedEventArgs e)
        {
            TaskProgressWindow taskProgressWindow = new TaskProgressWindow(TaskType.Distribution);
            taskProgressWindow.ShowDialog();
            listBox_CollectionItems.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string collectionName in CollectionStore.GetCollectionNames())
            {
                if (CollectionStore.Get(collectionName).IsChanged)
                {
                    if (MessageBox.Show("Текущие изменения не сохранены, закрыть?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        e.Cancel = true;
                    break;
                }
            }
        }

        private void MenuItem_RenameFile_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_CollectionNames.SelectedItem != null && listBox_CollectionItems.SelectedItem != null)
            {
                string currentCollectionName = (string)comboBox_CollectionNames.SelectedItem;
                string currentCollectionItem = (string)listBox_CollectionItems.SelectedItem;
                RenameFileWindow renameFileWindow = new RenameFileWindow(currentCollectionName, currentCollectionItem);
                renameFileWindow.ShowDialog();
                if (renameFileWindow.IsApply)
                    listBox_CollectionItems.Items.Refresh();
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
                    listBox_CollectionItems.Items.Refresh();
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
                    foreach (string item in listBox_CollectionItems.SelectedItems)
                    {
                        File.Delete($"{CollectionStore.BaseDirectory}\\{item}");
                        collection.RemovePermanently(item);
                    }
                    listBox_CollectionItems.Items.Refresh();
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
                    case Key.R:
                        MenuItem_EditCollectionDetails_Click(null, null);
                        break;
                    case Key.D:
                        MenuItem_DistributeFolders_Click(null, null);
                        break;
                    case Key.Delete:
                        MenuItem_RemoveAllSelectedFiles_Click(null, null);
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

        private void Grid_Image_SizeChanged(object sender, SizeChangedEventArgs e)
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
                //if (image_Image.Height != double.NaN)
                //{
                image_Image.Height = double.NaN;
                image_Image.Width = double.NaN;
                image_Image.Stretch = Stretch.Uniform;
                //}
            }
        }
    }
}
