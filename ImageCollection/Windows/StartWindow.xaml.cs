using ImageCollection.Classes.Settings;
using ImageCollection.Enums;
using System.Windows;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWork StartWork { get; private set; } = StartWork.None;

        public StartWindow()
        {
            InitializeComponent();

            Title = App.Name;

            ProgramSettings settings = ProgramSettings.GetInstance();
            if (string.IsNullOrEmpty(settings.LastOpenCollection))
            {
                button_LastOpenCollection.IsEnabled = false;
            }
        }

        private void Button_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            StartWork = StartWork.OpenFolder;
            Close();
        }

        private void Button_OpenCollections_Click(object sender, RoutedEventArgs e)
        {
            StartWork = StartWork.OpenCollection;
            Close();
        }

        private void Button_LastOpenCollections_Click(object sender, RoutedEventArgs e)
        {
            StartWork = StartWork.LastOpenCollection;
            Close();
        }
    }
}
