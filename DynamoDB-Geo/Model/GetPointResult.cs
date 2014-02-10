using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public sealed class GetPointResult : GeoDataResult
    {
        public GetItemResult GetItemResult { get; private set; }

        public GetPointResult(GetItemResult getItemResult)
        {
            if (getItemResult == null) throw new ArgumentNullException("getItemResult");
            GetItemResult = getItemResult;
        }
    }
}
