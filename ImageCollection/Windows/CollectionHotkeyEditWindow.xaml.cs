using ImageCollection.Classes.Collections;
using ImageCollection.Classes.Views;
using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly ObservableCollection<ComboBoxModifierKey> modifiersKeys;
        private readonly Brush currentForeground;
        private readonly Brush placeholderForeground;

        private Key? editkey = null;
        private Key? currentKey = null;

        public CollectionKeyInformation? KeyInformation { get; private set; } = null;

        public CollectionHotkeyEditWindow(Hotkey? hotkey = null)
        {
            InitializeComponent();

            currentForeground = (Brush)TryFindResource("Base.Foreground");
            placeholderForeground = (Brush)TryFindResource("Base.Placeholder.Foreground");

            modifiersKeys = new ObservableCollection<ComboBoxModifierKey> {
                new ComboBoxModifierKey { DisplayKey = "Нет", ValueKey = ModifierKeys.None },
                new ComboBoxModifierKey { DisplayKey = "Alt", ValueKey = ModifierKeys.Alt },
                new ComboBoxModifierKey { DisplayKey = "Ctrl", ValueKey = ModifierKeys.Control },
                new ComboBoxModifierKey { DisplayKey = "Shift", ValueKey = ModifierKeys.Shift}
            };
            comboBox_ModifierKey.ItemsSource = modifiersKeys;
            comboBox_ModifierKey.DisplayMemberPath = "DisplayKey";

            comboBox_Collections.ItemsSource = CollectionStore.ActualCollections;

            if (hotkey.HasValue)
            {
                comboBox_Collections.SelectedItem = CollectionStore.Settings.CollectionHotkeys[hotkey.Value];
                textBox_Hotkey.Text = hotkey.Value.ToString();
                editkey = hotkey.Value.Key;
                currentKey = hotkey.Value.Key;
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
            if (!currentKey.HasValue)
            {
                Classes.UI.MessageBox.Show("Введите горячую клавишу!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ModifierKeys modifier = ((ComboBoxModifierKey)comboBox_ModifierKey.SelectedItem).ValueKey;
            Hotkey hotkey = new Hotkey { Key = currentKey.Value, Modifier = modifier };
            if (CollectionStore.Settings.CollectionHotkeys.ContainsKey(hotkey) && !currentKey.Equals(editkey))
            {
                Classes.UI.MessageBox.Show("Такая комбинация уже используется, введите другую!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string collectionName = (string)comboBox_Collections.SelectedItem;
            if (CollectionStore.Settings.CollectionHotkeys.ContainsValue(collectionName))
            {
                if (Classes.UI.MessageBox.Show("Для выбранной коллекции уже назначена комбинация клавиш, назначить еще одну?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            KeyInformation = new CollectionKeyInformation(hotkey, collectionName);
            Close();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int keyInt = (int)e.Key;
            ModifierKeys modifier = ((ComboBoxModifierKey)comboBox_ModifierKey.SelectedItem).ValueKey;
            if ((keyInt >= 34 && keyInt <= 69) || (keyInt >= 74 && keyInt <= 83) || (keyInt >= 90 && keyInt <= 113))
            {
                if ((modifier == ModifierKeys.Control &&
                    e.Key != Key.O &&
                    e.Key != Key.S &&
                    e.Key != Key.F2 &&
                    e.Key != Key.N &&
                    e.Key != Key.E &&
                    e.Key != Key.D &&
                    e.Key != Key.H &&
                    e.Key != Key.A) ||
                    (modifier == ModifierKeys.Alt &&
                    e.Key != Key.F4) ||
                    modifier == ModifierKeys.None)
                {
                    textBox_Hotkey.Text = e.Key.ToString();
                    currentKey = e.Key;
                }
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

        private void СomboBox_ModifierKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textBox_Hotkey.Text = string.Empty;
            currentKey = null;
        }
    }
}