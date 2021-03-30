using ImageCollection.Classes.Static;
using System.Windows;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для SelectCollectionWindow.xaml
    /// </summary>
    public partial class SelectCollectionWindow : Window
    {
        private readonly string currentCollectionName;

        public bool IsApply { get; private set; } = false;
        
        public SelectCollectionWindow(string currentCollectionName)
        {
            InitializeComponent();
            Title = App.Name;
            comboBox_CollectionNames.ItemsSource = CollectionStore.GetCollectionNames();
            comboBox_CollectionNames.SelectedItem = currentCollectionName;
            this.currentCollectionName = currentCollectionName;
        }

        public string GetNameSelectedCollection() => (string)comboBox_CollectionNames.SelectedItem;

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            string nameSelectedCollection = (string)comboBox_CollectionNames.SelectedItem;
            if (!currentCollectionName.Equals(nameSelectedCollection))
                IsApply = true;
            Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}