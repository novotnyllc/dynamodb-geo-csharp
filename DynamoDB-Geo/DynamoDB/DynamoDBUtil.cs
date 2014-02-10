using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;


namespace Amazon.Geo.DynamoDB
{
    static class DynamoDBUtil
    {
        public static QueryRequest CopyQueryRequest(this QueryRequest queryRequest)
        {
            var copiedRequest = new QueryRequest
            {
                AttributesToGet = queryRequest.AttributesToGet.ToList(), // deep copy
                ConsistentRead = queryRequest.ConsistentRead,
                ExclusiveStartKey = queryRequest.ExclusiveStartKey.ToDictionary(kvp => kvp.Key, kvp =>kvp.Value), // deep copy
                IndexName = queryRequest.IndexName,
                KeyConditions = queryRequest.KeyConditions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Limit = queryRequest.Limit,
                ReturnConsumedCapacity = queryRequest.ReturnConsumedCapacity,
                ScanIndexForward = queryRequest.ScanIndexForward,
                Select = queryRequest.Select,
                TableName = queryRequest.TableName
            };

            return copiedRequest;
        }
    }
}
