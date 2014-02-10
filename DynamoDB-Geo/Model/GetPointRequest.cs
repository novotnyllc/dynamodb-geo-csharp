using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    /// <summary>
     /// Get point request. The request must specify a geo point and a range key value. You can modify GetItemRequest to
     /// customize the underlining Amazon DynamoDB get item request, but the table name, hash key, geohash, and geoJson
     /// attribute will be overwritten by GeoDataManagerConfiguration.
    /// </summary>
    public sealed class GetPointRequest : GeoDataRequest
    {
        public GeoPoint GeoPoint { get; private set; }
        public AttributeValue RangeKeyValue { get; private set; }
        public GetItemRequest GetItemRequest { get; private set; }

        public GetPointRequest(GeoPoint geoPoint, AttributeValue rangeKeyValue)
        {
            if (geoPoint == null) throw new ArgumentNullException("geoPoint");
            if (rangeKeyValue == null) throw new ArgumentNullException("rangeKeyValue");

            GetItemRequest = new GetItemRequest();
            GeoPoint = geoPoint;
            RangeKeyValue = rangeKeyValue;
            
        }
    }
}
