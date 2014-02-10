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
        private readonly ConcurrentBag<IDictionary<string, AttributeValue>> _item;
        private readonly ConcurrentBag<QueryResult> _queryResults;

        public GeoQueryResult()
        {
            _item = new ConcurrentBag<IDictionary<string, AttributeValue>>();
            _queryResults = new ConcurrentBag<QueryResult>();
        }

        public GeoQueryResult(GeoQueryResult geoQueryResult)
        {
            _item = geoQueryResult._item;
            _queryResults = geoQueryResult._queryResults;
        }

        public ConcurrentBag<IDictionary<string, AttributeValue>> Item { get { return _item; } }
        public ConcurrentBag<QueryResult> QueryResults { get { return _queryResults; } }
    }
}
