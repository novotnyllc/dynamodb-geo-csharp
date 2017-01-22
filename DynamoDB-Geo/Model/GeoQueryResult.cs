using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public class GeoQueryResult : GeoDataResult
    {
        private readonly ConcurrentBag<IDictionary<string, AttributeValue>> _items;
        private readonly ConcurrentBag<QueryResponse> _queryResults;

        public GeoQueryResult()
        {
            _items = new ConcurrentBag<IDictionary<string, AttributeValue>>();
            _queryResults = new ConcurrentBag<QueryResponse>();
        }

        public GeoQueryResult(GeoQueryResult geoQueryResult)
        {
            _items = geoQueryResult._items;
            _queryResults = geoQueryResult._queryResults;
        }

        public ConcurrentBag<IDictionary<string, AttributeValue>> Items { get { return _items; } }
        public ConcurrentBag<QueryResponse> QueryResults { get { return _queryResults; } }
    }
}
