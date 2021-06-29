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
        private const string EnterHotkeyPlaceholder = "Введите клавишу";

        private readonly Brush currentForeground;
        private readonly Brush placeholderForeground;
        private readonly string currentCollection = null;

        private Key? key = null;

        public CollectionKeyInformation? KeyInformation { get; private set; } = null;

        public CollectionHotkeyEditWindow(Key? key = null)
        {
            InitializeComponent();

            currentForeground = (Brush)TryFindResource("Base.Foreground");
            placeholderForeground = (Brush)TryFindResource("Base.Placeholder.Foreground");

            comboBox_Collections.ItemsSource = CollectionStore.ActualCollections;

            if (key.HasValue)
            {
                currentCollection = CollectionStore.Settings.CollectionHotkeys[key.Value];
                comboBox_Collections.SelectedItem = currentCollection;
                textBox_Hotkey.Text = key.Value.ToString();
            }
            else
            {
                textBox_Hotkey.Text = EnterHotkeyPlaceholder;
                textBox_Hotkey.Foreground = placeholderForeground;
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
            KeyInformation = new CollectionKeyInformation(key.Value, collectionName);
            Close();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl && 
                e.Key != Key.RightCtrl && 
                e.Key != Key.LeftShift && 
                e.Key != Key.RightShift && 
                e.Key != Key.LeftAlt && 
                e.Key != Key.RightAlt && 
                e.Key != Key.Enter && 
                e.Key != Key.O && 
                e.Key != Key.S &&
                e.Key != Key.Delete && 
                e.Key != Key.F2 && 
                e.Key != Key.N && 
                e.Key != Key.E && 
                e.Key != Key.D && 
                e.Key != Key.H &&
                e.Key != Key.A && 
                e.Key != Key.Up && 
                e.Key != Key.Down &&
                e.Key != Key.Tab)
            {
                textBox_Hotkey.Text = e.Key.ToString();
                key = e.Key;
            }
            e.Handled = true;
        }

        private void TextBox_Hotkey_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_Hotkey.Text.Equals(EnterHotkeyPlaceholder))
            {
                textBox_Hotkey.Foreground = currentForeground;
                textBox_Hotkey.Text = string.Empty;
            }
        }

        private void TextBox_Hotkey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Hotkey.Text))
            {
                textBox_Hotkey.Foreground = placeholderForeground;
                textBox_Hotkey.Text = EnterHotkeyPlaceholder;
            }
        }
    }
}