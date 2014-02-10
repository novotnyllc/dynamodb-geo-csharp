using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Geo.Model;

namespace Amazon.Geo.DynamoDB
{
    internal class DynamoDBManager
    {
        private readonly GeoDataManagerConfiguration _config;

        public DynamoDBManager(GeoDataManagerConfiguration config)
        {
            _config = config;
        }

        /**
	 * Query Amazon DynamoDB
	 * 
	 * @param hashKey
	 *            Hash key for the query request.
	 * 
	 * @param range
	 *            The range of geohashs to query.
	 * 
	 * @return The query result.
	 */

        public async Task<IReadOnlyList<QueryResult>> QueryGeohashAsync(QueryRequest queryRequest, ulong hashKey, GeohashRange range)
        {
            var queryResults = new List<QueryResult>();
            IDictionary<String, AttributeValue> lastEvaluatedKey = null;

            do
            {
                var keyConditions = new Dictionary<String, Condition>();

                var hashKeyCondition = new Condition
                {
                    ComparisonOperator = ComparisonOperator.EQ,
                    AttributeValueList = new List<AttributeValue>
                    {
                        new AttributeValue
                        {
                            N = hashKey.ToString(CultureInfo.InvariantCulture)
                        }
                    }
                };

                keyConditions.Add(_config.HashKeyAttributeName, hashKeyCondition);

                var minRange = new AttributeValue
                {
                    N = range.RangeMin.ToString(CultureInfo.InvariantCulture)
                };
                var maxRange = new AttributeValue
                {
                    N = range.RangeMax.ToString(CultureInfo.InvariantCulture)
                };

                var geohashCondition = new Condition
                {
                    ComparisonOperator = ComparisonOperator.BETWEEN,
                    AttributeValueList = new List<AttributeValue>
                    {
                        minRange,
                        maxRange
                    }
                };

                keyConditions.Add(_config.GeohashAttributeName, geohashCondition);

                queryRequest.TableName = _config.TableName;
                queryRequest.KeyConditions = keyConditions;
                queryRequest.IndexName = _config.GeohashIndexName;
                queryRequest.ConsistentRead = true;
                queryRequest.ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL;

                if (lastEvaluatedKey != null)
                {
                    queryRequest.ExclusiveStartKey[_config.HashKeyAttributeName] =
                        lastEvaluatedKey[_config.HashKeyAttributeName];
                }

                QueryResult queryResult = await _config.DynamoDBClient.QueryAsync(queryRequest).ConfigureAwait(false);
                queryResults.Add(queryResult);

                lastEvaluatedKey = queryResult.LastEvaluatedKey;
            } while (lastEvaluatedKey != null);

            return queryResults;
        }
    }
}