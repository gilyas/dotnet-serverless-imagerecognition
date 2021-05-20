using Common;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace extract_image_metadata
{
    public class ImageMetadata : ExecutionInput
    {
        public long Id { get; set; }

        public string Format { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public decimal OrignalImagePixelCount { get; set; }

        public long Size { get; set; }

        //public IReadOnlyCollection<IExifValue> ExifProfile { get; set; }

        public string ExifProfileBase64 => Convert.ToBase64String(ExifProfile.ToByteArray());

        public ExifProfile ExifProfile { get; set; }
    }
}
