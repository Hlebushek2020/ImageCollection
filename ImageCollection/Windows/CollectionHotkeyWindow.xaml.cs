using ImageCollection.Classes.Collections;
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
    /// Логика взаимодействия для CollectionHotkey.xaml
    /// </summary>
    public partial class CollectionHotkeyWindow : Window
    {

        public CollectionHotkeyWindow()
        {
            InitializeComponent();
            Title = App.Name;

            listBox_Hotkeys.ItemsSource = CollectionStore.Settings.CollectionHotkeys;
        }

        private void Button_AddHotkey_Click(object sender, RoutedEventArgs e)
        {
            CollectionHotkeyEditWindow collectionHotkeyEdit = new CollectionHotkeyEditWindow();
            collectionHotkeyEdit.ShowDialog();
            if (collectionHotkeyEdit.KeyInformation.HasValue)
            {
                CollectionKeyInformation keyInformation = collectionHotkeyEdit.KeyInformation.Value;
                CollectionStore.Settings.AddHotkey(keyInformation);
                listBox_Hotkeys.Items.Refresh();
            }
        }

        private void Button_EditHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_Hotkeys.SelectedItem != null)
            {
                KeyValuePair<Hotkey, string> item = (KeyValuePair<Hotkey, string>)listBox_Hotkeys.SelectedItem;
                CollectionHotkeyEditWindow collectionHotkeyEdit = new CollectionHotkeyEditWindow(item.Key);
                collectionHotkeyEdit.ShowDialog();
                if (collectionHotkeyEdit.KeyInformation.HasValue)
                {
                    CollectionKeyInformation keyInformation = collectionHotkeyEdit.KeyInformation.Value;
                    Dictionary<Hotkey, string> hotkeys = CollectionStore.Settings.CollectionHotkeys;
                    if (hotkeys.ContainsKey(keyInformation.Hotkey))
                    {
                        CollectionStore.Settings.SetHotkeyCollection(keyInformation);
                    }
                    else
                    {
                        CollectionStore.Settings.AddHotkey(keyInformation);
                    }
                    listBox_Hotkeys.Items.Refresh();
                }
            }
        }

        private void Button_RemoveHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_Hotkeys.SelectedItem != null)
            {
                KeyValuePair<Hotkey, string> item = (KeyValuePair<Hotkey, string>)listBox_Hotkeys.SelectedItem;
                if (Classes.UI.MessageBox.Show($"Удалить бинд {item.Key}?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CollectionStore.Settings.RemoveHotkey(item.Key);
                    listBox_Hotkeys.Items.Refresh();
                }
            }
        }

        private void Button_RemoveAllHotkeys_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_Hotkeys.Items.Count > 0)
            {
                if (Classes.UI.MessageBox.Show("Удалить все бинды?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CollectionStore.Settings.RemoveAllHotkeys();
                    listBox_Hotkeys.Items.Refresh();
                }
            }
        }

        private void ListBox_Hotkeys_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
            Button_EditHotkey_Click(null, null);
    }
}
