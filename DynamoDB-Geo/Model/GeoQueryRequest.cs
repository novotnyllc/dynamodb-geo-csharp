using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public class GeoQueryRequest : GeoDataRequest
    {
        public QueryRequest QueryRequest { get; private set; }

        public GeoQueryRequest()
        {
            QueryRequest = new QueryRequest();
        }

    }
}
