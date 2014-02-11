using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleServer.Client
{
    public class SchoolSearchResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string RangeKey { get; set; }
        public string SchoolName { get; set; }

    }
}