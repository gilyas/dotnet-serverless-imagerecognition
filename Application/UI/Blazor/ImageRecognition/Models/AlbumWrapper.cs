using ImageRecognition.API.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.BlazorFrontend.Models
{
    public class AlbumWrapper : INotifyPropertyChanged
    {

        public AlbumWrapper(Album album)
        {
            this.Album = album;
        }
        public Album Album { get; set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
