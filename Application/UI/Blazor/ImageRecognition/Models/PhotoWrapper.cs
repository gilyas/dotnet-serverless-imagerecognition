using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using ImageRecognition.API.Client;
using Newtonsoft.Json;

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

        public void Update(MessageEvent evnt)
        {
            if (string.Equals(Photo.PhotoId, evnt.ResourceId, StringComparison.Ordinal))
            {
                if (evnt.CompleteEvent)
                {
                    var photo = JsonConvert.DeserializeObject<Photo>(evnt.Data);

                    string signedThumbnailUrl = Photo.Thumbnail.Url;
                    string signedFullSizeUrl = Photo.FullSize.Url;

                    if (photo != null)
                    {
                        this.Photo = photo;
                    }

                    Photo.Thumbnail.Url = signedThumbnailUrl;
                    Photo.FullSize.Url = signedFullSizeUrl;
                    Photo.ProcessingStatus = ProcessingStatus.Succeeded;
                    Status = ProcessingStatus.Succeeded.ToString();
                }
                else
                {
                    Status = evnt.Message;
                }
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
