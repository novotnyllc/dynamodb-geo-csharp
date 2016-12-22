using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public sealed class UpdatePointResult : GeoDataResult
    {
        public UpdateItemResponse UpdateItemResult { get; private set; }

        public UpdatePointResult(UpdateItemResponse updateItemResult)
        {
            if (updateItemResult == null) throw new ArgumentNullException("updateItemResult");
            UpdateItemResult = updateItemResult;
        }
    }
}
