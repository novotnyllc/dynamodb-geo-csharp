using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Geo.Model;

namespace SampleServer.Models
{
    public class RectangleQuery
    {
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLng { get; set; }
        public double MaxLng { get; set; }
    }
}