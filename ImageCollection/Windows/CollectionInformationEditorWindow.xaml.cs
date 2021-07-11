using ImageCollection.Classes.Collections;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для EditCollectionNameWindow.xaml
    /// </summary>
    public partial class CollectionInformationEditorWindow : Window
    {
        private const string CollectionNamePlaceholder = "Название коллекции";
        private const string CollectionDescriptionPlaceholder = "Описание коллекции (Не обязательно)";

        private readonly Brush currentForeground;
        private readonly Brush placeholderForeground;

        private readonly string collectionName = string.Empty;
        private readonly string collectionDescription = string.Empty;
        private bool changedCollectionName = false;
        private bool changedCollectionDescription = false;

        public CollectionInformationEditorWindow()
        {
            InitializeComponent();
            Title = App.Name;

            currentForeground = (Brush)TryFindResource("Base.Foreground");
            placeholderForeground = (Brush)TryFindResource("Base.Placeholder.Foreground");

            textBox_collectionName.Foreground = placeholderForeground;
            textBox_collectionName.Text = CollectionNamePlaceholder;
            textBox_collectionDescription.Foreground = placeholderForeground;
            textBox_collectionDescription.Text = CollectionDescriptionPlaceholder;
        }

        public CollectionInformationEditorWindow(string collectionName, string collectionDescription)
        {
            InitializeComponent();
            Title = App.Name;
            this.collectionName = collectionName;
            textBox_collectionName.Text = collectionName;
            if (collectionName.Equals(CollectionStore.BaseCollectionName))
                textBox_collectionName.IsEnabled = false;
            if (!string.IsNullOrEmpty(collectionDescription))
            {
                this.collectionDescription = collectionDescription;
                textBox_collectionDescription.Text = collectionDescription;
            }
            else
            {
                textBox_collectionDescription.Foreground = placeholderForeground;
                textBox_collectionDescription.Text = CollectionDescriptionPlaceholder;
            }
        }

        public CollectionInformation GetCollectionInformation() => 
            new CollectionInformation(textBox_collectionName.Text, changedCollectionName, textBox_collectionDescription.Text, changedCollectionDescription);

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            string newCollectionName = textBox_collectionName.Text;
            if (newCollectionName.Equals(CollectionNamePlaceholder))
            {
                Classes.UI.MessageBox.Show("Название коллекции не может быть пустым!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Regex regex = new Regex(".*[\\<>:\"/|?*].*");
            if (regex.IsMatch(newCollectionName))
            {
                Classes.UI.MessageBox.Show("Название коллекции содержит запрещенные символы! (< > : \" \\ / | ? *)", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!collectionName.Equals(newCollectionName))
            {
                if (CollectionStore.Contains(newCollectionName))
                {
                    Classes.UI.MessageBox.Show("Коллекция с таким названием уже существует!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                changedCollectionName = true;
            }
            if (!textBox_collectionDescription.Text.Equals(collectionDescription))
            {
                changedCollectionDescription = true;
                if (textBox_collectionDescription.Text.Equals(CollectionDescriptionPlaceholder))
                    textBox_collectionDescription.Text = string.Empty;
            }
            Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TextBox_EditCollectionName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_collectionName.Text.Equals(CollectionNamePlaceholder))
            {
                textBox_collectionName.Foreground = currentForeground;
                textBox_collectionName.Text = string.Empty;
            }
        }

        private void TextBox_EditCollectionName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_collectionName.Text))
            {
                textBox_collectionName.Foreground = placeholderForeground;
                textBox_collectionName.Text = CollectionNamePlaceholder;
            }
        }

        private void TextBox_Description_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_collectionDescription.Text.Equals(CollectionDescriptionPlaceholder))
            {
                textBox_collectionDescription.Foreground = currentForeground;
                textBox_collectionDescription.Text = string.Empty;
            }
        }

        private void TextBox_Description_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_collectionDescription.Text))
            {
                textBox_collectionDescription.Foreground = placeholderForeground;
                textBox_collectionDescription.Text = CollectionDescriptionPlaceholder;
            }
        }
    }
}
