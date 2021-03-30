using ImageCollection.Structures;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для SettingsOpenFolderWindow.xaml
    /// </summary>
    public partial class SettingsOpenFolderWindow : Window
    {
        private const string BaseDirectoryPlaceholder = "Базовая директория";
        private const string DistributionFolderPlaceholder = "Директория для размещения";

        private bool isOpenFolder = false;

        public SettingsOpenFolderWindow()
        {
            InitializeComponent();
            Title = App.Name;
            textBox_baseDirectory.Text = BaseDirectoryPlaceholder;
            textBox_distributionNewFolder.Text = DistributionFolderPlaceholder;
        }

        private void Button_BaseDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!textBox_baseDirectory.Text.Equals(BaseDirectoryPlaceholder))
                    folderBrowserDialog.SelectedPath = textBox_baseDirectory.Text;
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox_baseDirectory.Foreground = Brushes.Black;
                    textBox_baseDirectory.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void CheckBox_IsDistributionNewFolder_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = checkBox_isDistributionNewFolder.IsChecked.Value;
            textBox_distributionNewFolder.IsEnabled = enabled;
            if (!textBox_distributionNewFolder.Text.Equals(DistributionFolderPlaceholder))
            {
                if (enabled)
                    textBox_distributionNewFolder.Foreground = Brushes.Black;
                else
                    textBox_distributionNewFolder.Foreground = Brushes.Gray;
            }
            button_distributionNewFolder.IsEnabled = enabled;
        }

        private void Button_DistributionNewFolder_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!textBox_distributionNewFolder.Text.Equals(DistributionFolderPlaceholder))
                    folderBrowserDialog.SelectedPath = textBox_distributionNewFolder.Text;
                string path = textBox_distributionNewFolder.Text;
                bool success = false;
                do
                {
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = folderBrowserDialog.SelectedPath;
                        if (path.Equals(textBox_baseDirectory.Text))
                            MessageBox.Show("Директория для размещения не может совпадать с базовой директорией! Выберите другую директорию для размещения или уберите флажок в соответствующем пункте.",
                                App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        else
                            success = true;
                    }
                    else break;
                } while (!success);
                if (success)
                {
                    textBox_distributionNewFolder.Text = path;
                    textBox_distributionNewFolder.Foreground = Brushes.Black;
                }
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_baseDirectory.Text.Equals(BaseDirectoryPlaceholder))
            {
                MessageBox.Show("Выберите базавую директорию!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(textBox_searchMask.Text))
            {
                MessageBox.Show("Маска для первого поиска не должна быть пустой!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DistributionFolderPlaceholder.Equals(textBox_baseDirectory.Text) && checkBox_isDistributionNewFolder.IsChecked.Value)
            {
                MessageBox.Show("Директория для размещения не может совпадать с базовой директорией! Выберите другую директорию для размещения или уберите флажок в соответствующем пункте.",
                    App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!checkBox_isDistributionNewFolder.IsChecked.Value)
            {
                if (MessageBox.Show("ВНИМАНИЕ! По умолчанию при распределении происходит перемещение файлов, для копирования при первом распределении нужно отметить соответствующий пункт.", App.Name, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    return;
            }
            isOpenFolder = true;
            Close();
        }

        public OpenFolderArgs GetArgs()
        {
            if (checkBox_isDistributionNewFolder.IsChecked.Value)
                return new OpenFolderArgs(isOpenFolder, textBox_baseDirectory.Text, checkBox_recursiveSearch.IsChecked.Value,
                    textBox_searchMask.Text, textBox_distributionNewFolder.Text);
            return new OpenFolderArgs(isOpenFolder, textBox_baseDirectory.Text, checkBox_recursiveSearch.IsChecked.Value, textBox_searchMask.Text, null);
        }
    }
}
