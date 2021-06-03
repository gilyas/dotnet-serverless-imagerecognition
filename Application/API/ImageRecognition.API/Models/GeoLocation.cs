using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.API.Models
{
    public class GeoLocation
    {
        public Coordinate Latitude { get; set; }

        public Coordinate Longtitude { get; set; }

        public override string ToString()
        {
            return $"{Latitude.D}°{Math.Round(Latitude.M)}'{Math.Round(Latitude.S)}''{Latitude.Direction}" +
                $" {Longtitude.D}°{Math.Round(Longtitude.M)}'{Math.Round(Longtitude.S)}''{Longtitude.Direction}";
        }
    }
}
