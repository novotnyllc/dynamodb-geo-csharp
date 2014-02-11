using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace SampleServer.Models
{
    public class RadiusQuery
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }

        [Required]
        [Range(.01, 6367000)]
        public double RadiusInMeters { get; set; } 
    }
}