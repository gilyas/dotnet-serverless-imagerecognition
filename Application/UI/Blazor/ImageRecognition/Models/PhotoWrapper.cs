using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using ImageRecognition.API.Client;

namespace ImageRecognition.BlazorFrontend.Models
{
    public class PhotoWrapper : INotifyPropertyChanged
    {

        public PhotoWrapper(Photo photo)
        {
            this.Photo = photo;
            this._status = this.Photo.ProcessingStatus.ToString();
        }
        public Photo Photo { get; set; }

        string _status;
        public string Status
        {
            get
            {
                if (this.Photo.ProcessingStatus == ProcessingStatus.Failed)
                {
                    return $"Failed";
                }
                return this._status;
            }
            set
            {
                this._status = value;
                OnPropertyChanged("Status");
            }
        }


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
