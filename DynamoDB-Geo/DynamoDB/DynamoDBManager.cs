using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Geo.Model;
using Amazon.Geo.S2;
using Amazon.Geo.Util;

namespace Amazon.Geo.DynamoDB
{
    internal sealed class DynamoDBManager
    {
        private readonly GeoDataManagerConfiguration _config;

        public DynamoDBManager(GeoDataManagerConfiguration config)
        {
            _config = config;
        }


        public async Task<DeletePointResult> DeletePointAsync(DeletePointRequest deletePointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (deletePointRequest == null) throw new ArgumentNullException("deletePointRequest");

            var geohash = S2Manager.GenerateGeohash(deletePointRequest.GeoPoint);
            var hashKey = S2Manager.GenerateHashKey(geohash, _config.HashKeyLength);

            var deleteItemRequest = deletePointRequest.DeleteItemRequest;

            deleteItemRequest.TableName = _config.TableName;

            var hashKeyValue = new AttributeValue
            {
                N = hashKey.ToString(CultureInfo.InvariantCulture)
            };

            deleteItemRequest.Key[_config.HashKeyAttributeName] = hashKeyValue;
            deleteItemRequest.Key[_config.RangeKeyAttributeName] = deletePointRequest.RangeKeyValue;

            DeleteItemResult deleteItemResult = await _config.DynamoDBClient.DeleteItemAsync(deleteItemRequest, cancellationToken).ConfigureAwait(false);
            var deletePointResult = new DeletePointResult(deleteItemResult);

            return deletePointResult;
        }

        public async Task<UpdatePointResult> UpdatePointAsync(UpdatePointRequest updatePointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (updatePointRequest == null) throw new ArgumentNullException("updatePointRequest");

            var geohash = S2Manager.GenerateGeohash(updatePointRequest.GeoPoint);
            var hashKey = S2Manager.GenerateHashKey(geohash, _config.HashKeyLength);

            var updateItemRequest = updatePointRequest.UpdateItemRequest;
            updateItemRequest.TableName = _config.TableName;

            var hashKeyValue = new AttributeValue
            {
                N = hashKey.ToString(CultureInfo.InvariantCulture)
            };

            updateItemRequest.Key[_config.HashKeyAttributeName] = hashKeyValue;
            updateItemRequest.Key[_config.RangeKeyAttributeName] = updatePointRequest.RangeKeyValue;

            // Geohash and geoJson cannot be updated.
            updateItemRequest.AttributeUpdates.Remove(_config.GeohashAttributeName);
            updateItemRequest.AttributeUpdates.Remove(_config.GeoJsonAttributeName);

            UpdateItemResult updateItemResult = await _config.DynamoDBClient.UpdateItemAsync(updateItemRequest, cancellationToken).ConfigureAwait(false);
            var updatePointResult = new UpdatePointResult(updateItemResult);

            return updatePointResult;
        }

        public async Task<GetPointResult> GetPointAsync(GetPointRequest getPointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (getPointRequest == null) throw new ArgumentNullException("getPointRequest");
            var geohash = S2Manager.GenerateGeohash(getPointRequest.GeoPoint);
            var hashKey = S2Manager.GenerateHashKey(geohash, _config.HashKeyLength);

            var getItemRequest = getPointRequest.GetItemRequest;
            getItemRequest.TableName = _config.TableName;

            var hashKeyValue = new AttributeValue
            {
                N = hashKey.ToString(CultureInfo.InvariantCulture)
            };
            getItemRequest.Key[_config.HashKeyAttributeName] = hashKeyValue;
            getItemRequest.Key[_config.RangeKeyAttributeName] = getPointRequest.RangeKeyValue;

            GetItemResult getItemResult = await _config.DynamoDBClient.GetItemAsync(getItemRequest, cancellationToken).ConfigureAwait(false);
            var getPointResult = new GetPointResult(getItemResult);

            return getPointResult;
        }

        public async Task<PutPointResult> PutPointAsync(PutPointRequest putPointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (putPointRequest == null) throw new ArgumentNullException("putPointRequest");

            var geohash = S2Manager.GenerateGeohash(putPointRequest.GeoPoint);
            var hashKey = S2Manager.GenerateHashKey(geohash, _config.HashKeyLength);
            var geoJson = GeoJsonMapper.StringFromGeoObject(putPointRequest.GeoPoint);

            var putItemRequest = putPointRequest.PutItemRequest;
            putItemRequest.TableName = _config.TableName;

            var hashKeyValue = new AttributeValue
            {
                N = hashKey.ToString(CultureInfo.InvariantCulture)
            };
            putItemRequest.Item[_config.HashKeyAttributeName] = hashKeyValue;
            putItemRequest.Item[_config.RangeKeyAttributeName] = putPointRequest.RangeKeyValue;


            var geohashValue = new AttributeValue
            {
                N = geohash.ToString(CultureInfo.InvariantCulture)
            };

            putItemRequest.Item[_config.GeohashAttributeName] = geohashValue;

            var geoJsonValue = new AttributeValue
            {
                S = geoJson
            };

            putItemRequest.Item[_config.GeoJsonAttributeName] = geoJsonValue;

            PutItemResult putItemResult = await _config.DynamoDBClient.PutItemAsync(putItemRequest, cancellationToken).ConfigureAwait(false);
            var putPointResult = new PutPointResult(putItemResult);

            return putPointResult;
        }

        /// <summary>
        ///     Query Amazon DynamoDB
        /// </summary>
        /// <param name="queryRequest"></param>
        /// <param name="hashKey">Hash key for the query request.</param>
        /// <param name="range">The range of geohashs to query.</param>
        /// <returns>The query result.</returns>
        public async Task<IReadOnlyList<QueryResult>> QueryGeohashAsync(QueryRequest queryRequest, ulong hashKey, GeohashRange range, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (queryRequest == null) throw new ArgumentNullException("queryRequest");
            if (range == null) throw new ArgumentNullException("range");

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

                if (lastEvaluatedKey != null && lastEvaluatedKey.Count > 0)
                {
                    queryRequest.ExclusiveStartKey[_config.HashKeyAttributeName] =
                        lastEvaluatedKey[_config.HashKeyAttributeName];
                }

                QueryResult queryResult = await _config.DynamoDBClient.QueryAsync(queryRequest, cancellationToken).ConfigureAwait(false);
                queryResults.Add(queryResult);

                lastEvaluatedKey = queryResult.LastEvaluatedKey;
            } while (lastEvaluatedKey != null && lastEvaluatedKey.Count > 0);

            return queryResults;
        }
    }
}