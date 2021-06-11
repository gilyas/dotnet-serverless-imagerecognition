using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.BlazorFrontend.Frontend
{
    public class AppOptions
    {
        public string ImageRecognitionApiUrl { get; set; }

        public string WebSocketApi { get; set; }

        public string PhotoStorageBucket { get; set; }

        public string UploadBucketPrefix { get; set; }
    }
}
