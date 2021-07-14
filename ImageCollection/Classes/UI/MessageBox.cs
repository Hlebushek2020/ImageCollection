using System.Windows;

namespace ImageCollection.Classes.UI
{
    public sealed class MessageBox
    {
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBoxWindow messageBox = new MessageBoxWindow(messageBoxText, caption, button, icon);
            messageBox.ShowDialog();
            return messageBox.Result;
        }
    }
}
