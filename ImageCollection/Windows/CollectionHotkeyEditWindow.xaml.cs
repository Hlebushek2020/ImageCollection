using ImageCollection.Classes.Collections;
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
    /// Логика взаимодействия для CollectionHotkeyEditWindow.xaml
    /// </summary>
    public partial class CollectionHotkeyEditWindow : Window
    {
        private readonly string currentCollection = null;
        private Key? key = null;

        public CollectionKeyInformation? KeyInformation { get; private set; } = null;

        public CollectionHotkeyEditWindow(Key? key = null)
        {
            InitializeComponent();
            comboBox_Collections.ItemsSource = CollectionStore.ActualCollections;
            if (key.HasValue)
            {
                currentCollection = CollectionStore.Settings.CollectionHotkeys[key.Value];
                comboBox_Collections.SelectedItem = currentCollection;
                textBox_Hotkey.Text = key.Value.ToString();
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!key.HasValue)
            {
                MessageBox.Show("Введите горячую клавишу!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CollectionStore.Settings.CollectionHotkeys.ContainsKey(key.Value))
            {
                MessageBox.Show("Такая горячая клавиша уже используется, введите другую!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string collectionName = (string)comboBox_Collections.SelectedItem;
            if (CollectionStore.Settings.CollectionHotkeys.ContainsValue(collectionName) && collectionName.Equals(currentCollection))
            {
                if (MessageBox.Show("Для такой коллекции уже назначена грячая клавиша, назначить еще одну?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            KeyInformation = new CollectionKeyInformation(key.Value, collectionName, !string.IsNullOrEmpty(currentCollection));
            Close();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl || e.Key != Key.RightCtrl || e.Key != Key.LeftShift || e.Key != Key.RightShift || e.Key != Key.LeftAlt
                || e.Key != Key.RightAlt || e.Key != Key.Enter)
            {
                textBox_Hotkey.Text = e.Key.ToString();
                key = e.Key;
                e.Handled = true;
            }
        }
    }
}