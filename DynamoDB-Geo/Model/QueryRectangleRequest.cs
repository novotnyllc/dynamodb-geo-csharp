using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Geo.Model
{
    public sealed class QueryRectangleRequest : GeoQueryRequest
    {
        public GeoPoint MinPoint { get; private set; }
        public GeoPoint MaxPoint { get; private set; }

        public QueryRectangleRequest(GeoPoint minPoint, GeoPoint maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }
    }
}
