using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleServer.Models
{
    public class RadiusQuery
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public double RadiusInMeters { get; set; } 
    }
}