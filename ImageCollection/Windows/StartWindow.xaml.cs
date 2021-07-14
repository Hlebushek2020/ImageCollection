using ImageCollection.Classes.Settings;
using ImageCollection.Enums;
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
