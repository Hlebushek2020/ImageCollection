using ImageCollection.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageCollection.Classes
{
    public class ListBoxImageItem : INotifyPropertyChanged
    {
        private string path;
        private BitmapImage bitmapImage;
        private readonly CollectionItemMeta itemMeta;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        } 

        public BitmapImage Preview
        {
            get => bitmapImage;
            set
            {
                bitmapImage = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => itemMeta.Description;
            set
            {
                itemMeta.Description = value;
                OnPropertyChanged();
            }
        }

        public string Hash
        {
            get => itemMeta.Hash;
            set => itemMeta.Hash = value;
        }

        public ListBoxImageItem(string path, CollectionItemMeta itemMeta)
        {
            this.path = path;
            this.itemMeta = itemMeta;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
