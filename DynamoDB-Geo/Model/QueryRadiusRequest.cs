using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Geo.Model
{
    public class QueryRadiusRequest : GeoQueryRequest
    {
        public GeoPoint CenterPoint { get; private set; }
        public double RadiusInMeter { get; private set; }

        public QueryRadiusRequest(GeoPoint centerPoint, double radiusInMeter)
        {
            CenterPoint = centerPoint;
            RadiusInMeter = radiusInMeter;
        }
    }
}
