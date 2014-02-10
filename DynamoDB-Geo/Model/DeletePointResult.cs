using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Model
{
    public sealed class DeletePointResult : GeoDataResult
    {
        public DeletePointResult(DeleteItemResult deleteItemResult)
        {
            if (deleteItemResult == null) throw new ArgumentNullException("deleteItemResult");

            DeleteItemResult = deleteItemResult;
        }

        public DeleteItemResult DeleteItemResult { get; private set; }
    }
}
