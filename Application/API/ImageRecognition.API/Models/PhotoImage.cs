using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.API.Models
{
    public class PhotoImage
    {
        public string Height { get; set; }

        public string Width { get; set; }

        public string Key { get; set; }

        public string Url { get; set; }
    }
}
