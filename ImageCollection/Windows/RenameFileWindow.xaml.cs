using ImageCollection.Classes;
using ImageCollection.Classes.Static;
using ImageCollection.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private readonly string oldFileName;
        private readonly string collectionName;
        private readonly string placeholder;
        private readonly bool isFile;

        public RenameFileWindow(string collectionName)
        {
            InitializeComponent();
            Title = App.Name;
            isFile = false;
            this.collectionName = collectionName;
            placeholder = NewFileMaskNamePlaceholder;
            textBox_NewFileName.Foreground = Brushes.Gray;
            textBox_NewFileName.Text = placeholder;
        }

        public RenameFileWindow(string collectionName, string item)
        {
            InitializeComponent();
            Title = App.Name;
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
                textBox_NewFileName.Foreground = Brushes.Black;
                textBox_NewFileName.Text = string.Empty;
            }
        }

        private void TextBox_NewFileName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_NewFileName.Text))
            {
                textBox_NewFileName.Foreground = Brushes.Gray;
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
            string toPath = $"{CollectionStore.BaseDirectory}\\{newFileName}";
            if (File.Exists(toPath))
            {
                MessageBox.Show("Файл с таким именем уже существует!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                Collection collection = CollectionStore.Get(collectionName);
                File.Move($"{CollectionStore.BaseDirectory}\\{oldFileName}", toPath);
                collection.RenameItem(oldFileName, newFileName);
                NewFileName = newFileName;
                IsApply = true;
                try
                {
                    MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                    byte[] oldPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(oldFileName));
                    byte[] newPreviewNameB = MD5.ComputeHash(Encoding.UTF8.GetBytes(newFileName));
                    StringBuilder oldPreviewNameS = new StringBuilder(oldPreviewNameB.Length * 2);
                    StringBuilder newPreviewNameS = new StringBuilder(newPreviewNameB.Length * 2);
                    for (int i = 0; i < oldPreviewNameB.Length; i++)
                    {
                        oldPreviewNameS.Append(oldPreviewNameB[i].ToString("X2"));
                        newPreviewNameS.Append(newPreviewNameB[i].ToString("X2"));
                    }
                    File.Move($"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{oldPreviewNameS}.jpg",
                        $"{CollectionStore.BaseDirectory}//{CollectionStore.DataDirectoryName}//preview//{newPreviewNameS}.jpg");
                }
                catch { }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, App.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            TaskProgressWindow progressWindow = new TaskProgressWindow(TaskType.RenameCollectionItems, new string[] { collectionName,  maskFileName });
            progressWindow.ShowDialog();
            IsApply = true;
            Close();
        }
    }
}
