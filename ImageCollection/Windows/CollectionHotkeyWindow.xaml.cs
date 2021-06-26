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
                CollectionStore.Settings.CollectionHotkeys.Add(keyInformation.Key, keyInformation.CollectionName);
                listBox_Hotkeys.Items.Refresh();
            }
        }
    }
}
