using ImageCollection.Classes.Collections;
using ImageCollection.Enums;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для RenameFileWindow.xaml
    /// </summary>
    public partial class RenameFileWindow : Window
    {
        private const string NewFileNamePlaceholder = "Новое имя файла";
        private const string NewFileMaskNamePlaceholder = "Маска имени файла";

        public bool IsApply { get; private set; } = false;
        public string NewFileName { get; private set; }

        private readonly Brush currentForeground;
        private readonly Brush placeholderForeground;

        private readonly string oldFileName;
        private readonly string collectionName;
        private readonly string placeholder;
        private readonly bool isFile;

        public RenameFileWindow(string collectionName)
        {
            InitializeComponent();
            Title = App.Name;

            currentForeground = (Brush)TryFindResource("Base.Foreground");
            placeholderForeground = (Brush)TryFindResource("Base.Placeholder.Foreground");

            isFile = false;
            this.collectionName = collectionName;
            placeholder = NewFileMaskNamePlaceholder;
            textBox_NewFileName.Foreground = placeholderForeground;
            textBox_NewFileName.Text = placeholder;
        }

        public RenameFileWindow(string collectionName, string item)
        {
            InitializeComponent();
            Title = App.Name;

            currentForeground = (Brush)TryFindResource("Base.Foreground");
            placeholderForeground = (Brush)TryFindResource("Base.Placeholder.Foreground");

            isFile = true;
            placeholder = NewFileNamePlaceholder;
            oldFileName = item;
            this.collectionName = collectionName;
            textBox_NewFileName.Text = Path.GetFileNameWithoutExtension(item);
        }

        private void TextBox_NewFileName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_NewFileName.Text.Equals(placeholder))
            {
                textBox_NewFileName.Foreground = currentForeground;
                textBox_NewFileName.Text = string.Empty;
            }
        }

        private void TextBox_NewFileName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_NewFileName.Text))
            {
                textBox_NewFileName.Foreground = placeholderForeground;
                textBox_NewFileName.Text = placeholder;
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (isFile)
                RenameFile();
            else
                RenameAllFiles();
        }

        private void RenameFile()
        {
            string newFileName = textBox_NewFileName.Text;
            if (newFileName.Equals(placeholder))
            {
                MessageBox.Show("Имя файла не может быть пустым!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Regex regex = new Regex(".*[\\<>:\"/|?*].*");
            if (regex.IsMatch(newFileName))
            {
                MessageBox.Show("Имя файла содержит запрещенные символы! (< > : \" \\ / | ? *)", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string dirName = Path.GetDirectoryName(oldFileName);
            newFileName = $"{(string.IsNullOrEmpty(dirName) ? "" : $"{dirName}\\")}{newFileName}{Path.GetExtension(oldFileName)}";
            string toPath = $"{CollectionStore.Settings.BaseDirectory}\\{newFileName}";
            if (File.Exists(toPath))
            {
                MessageBox.Show("Файл с таким именем уже существует!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Collection collection = CollectionStore.Get(collectionName);
            File.Move($"{CollectionStore.Settings.BaseDirectory}\\{oldFileName}", toPath);
            collection.Rename(oldFileName, newFileName);
            collection.IsChanged = true;
            NewFileName = newFileName;
            IsApply = true;
            // processing generate hash and rename preview
            CollectionItemMeta itemMeta = collection[newFileName];
            if (!string.IsNullOrEmpty(itemMeta.Hash))
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] newHash = md5.ComputeHash(Encoding.UTF8.GetBytes(newFileName));
                StringBuilder newHashSB = new StringBuilder(newHash.Length * 2);
                for (int i = 0; i < newHash.Length; i++)
                {
                    newHashSB.Append(newHash[i].ToString("X2"));
                }
                string newHashS = newHashSB.ToString();
                string previewDirectory = Path.Combine(CollectionStore.Settings.BaseDirectory, CollectionStore.DataDirectoryName, CollectionStore.PreviewDirectoryName);
                try
                {
                    File.Move(Path.Combine(previewDirectory, $"{itemMeta.Hash}.jpg"), Path.Combine(previewDirectory, $"{newHashS}.jpg"));
                    itemMeta.Hash = newHashS;
                }
                catch { }
            }
            Close();
        }

        private void RenameAllFiles()
        {
            string maskFileName = textBox_NewFileName.Text;
            if (maskFileName.Equals(placeholder))
            {
                MessageBox.Show("Маска файла не может быть пустой!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Regex regex = new Regex(".*[\\<>:\"/|?*].*");
            if (regex.IsMatch(maskFileName))
            {
                MessageBox.Show("Маска файла содержит запрещенные символы! (< > : \" \\ / | ? *)", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!maskFileName.Contains("{0}"))
            {
                MessageBox.Show("В маске файла отсутствует комбинация символов для подстановки номера! ({0})", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TaskProgressWindow progressWindow = new TaskProgressWindow(TaskType.RenameCollectionItems, new string[] { collectionName, maskFileName });
            progressWindow.ShowDialog();
            IsApply = true;
            Close();
        }
    }
}
