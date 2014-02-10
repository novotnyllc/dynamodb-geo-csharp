using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public sealed class PutPointResult : GeoDataResult
    {
        public PutItemResult PutItemResult { get; private set; }

        public PutPointResult(PutItemResult putItemResult)
        {
            if (putItemResult == null) throw new ArgumentNullException("putItemResult");
            PutItemResult = putItemResult;
        }
    }
}
