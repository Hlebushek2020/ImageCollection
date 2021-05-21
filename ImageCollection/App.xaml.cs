using ImageCollection.Classes.Settings;
using System;
using System.Windows;

namespace ImageCollection
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "Image Collection";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // set theme
            Uri uri = new Uri($"Themes/{ProgramSettings.GetInstance().Theme}.xaml", UriKind.Relative);
            ResourceDictionary resource = (ResourceDictionary)LoadComponent(uri);
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.Add(resource);
        }
    }
}
