using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.Frontend.Models
{
    public class Album
    {
        public string Id { get; set; }

        public string Owner { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
