using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Geo.DynamoDB;
using Amazon.Geo.Model;
using Amazon.Geo.S2;
using Amazon.Geo.Util;
using Google.Common.Geometry;

namespace Amazon.Geo
{
    public sealed class GeoDataManager
    {
        private readonly GeoDataManagerConfiguration _config;
        private readonly DynamoDBManager _dynamoDBManager;

        public GeoDataManager(GeoDataManagerConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");
            _config = config;

            _dynamoDBManager = new DynamoDBManager(_config);
        }

        public GeoDataManagerConfiguration GeoDataManagerConfiguration
        {
            get { return _config; }
        }

        /// <summary>
        ///     <p>
        ///         Delete a point from the Amazon DynamoDB table.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint geoPoint = new GeoPoint(47.5, -122.3);
        ///         String rangeKey = &quot;a6feb446-c7f2-4b48-9b3a-0f87744a5047&quot;;
        ///         AttributeValue rangeKeyValue = new AttributeValue().withS(rangeKey);
        ///         DeletePointRequest deletePointRequest = new DeletePointRequest(geoPoint, rangeKeyValue);
        ///         DeletePointResult deletePointResult = geoIndexManager.deletePoint(deletePointRequest);
        ///     </pre>
        /// </summary>
        /// <param name="deletePointRequest">Container for the necessary parameters to execute delete point request.</param>
        /// <returns>Result of delete point request.</returns>
        public Task<DeletePointResult> DeletePointAsync(DeletePointRequest deletePointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (deletePointRequest == null) throw new ArgumentNullException("deletePointRequest");
            return _dynamoDBManager.DeletePointAsync(deletePointRequest, cancellationToken);
        }

        /// <summary>
        ///     <p>
        ///         Query a circular area constructed by a center point and its radius.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint centerPoint = new GeoPoint(47.5, -122.3);
        ///         QueryRadiusRequest queryRadiusRequest = new QueryRadiusRequest(centerPoint, 100);
        ///         QueryRadiusResult queryRadiusResult = geoIndexManager.queryRadius(queryRadiusRequest);
        ///         for (Map&lt;String, AttributeValue&gt; item : queryRadiusResult.getItem()) {
        ///         System.out.println(&quot;item: &quot; + item);
        ///         }
        ///     </pre>
        /// </summary>
        /// <param name="queryRadiusRequest">Container for the necessary parameters to execute radius query request.</param>
        /// <returns>Result of radius query request.</returns>
        public async Task<QueryRadiusResult> QueryRadiusAsync(QueryRadiusRequest queryRadiusRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (queryRadiusRequest == null) throw new ArgumentNullException("queryRadiusRequest");
            if (queryRadiusRequest.RadiusInMeter <= 0 || queryRadiusRequest.RadiusInMeter > S2LatLng.EarthRadiusMeters)
                throw new ArgumentOutOfRangeException("queryRadiusRequest", "RadiusInMeter needs to be > 0  and <= " + S2LatLng.EarthRadiusMeters);

            var latLngRect = S2Util.GetBoundingLatLngRect(queryRadiusRequest);

            var cellUnion = S2Manager.FindCellIds(latLngRect);

            var ranges = MergeCells(cellUnion);

            var result = await DispatchQueries(ranges, queryRadiusRequest, cancellationToken).ConfigureAwait(false);
            return new QueryRadiusResult(result);
        }

        /// <summary>
        ///     <p>
        ///         Get a point from the Amazon DynamoDB table.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint geoPoint = new GeoPoint(47.5, -122.3);
        ///         AttributeValue rangeKeyValue = new AttributeValue().withS(&quot;a6feb446-c7f2-4b48-9b3a-0f87744a5047&quot;);
        ///         GetPointRequest getPointRequest = new GetPointRequest(geoPoint, rangeKeyValue);
        ///         GetPointResult getPointResult = geoIndexManager.getPoint(getPointRequest);
        ///         System.out.println(&quot;item: &quot; + getPointResult.getGetItemResult().getItem());
        ///     </pre>
        /// </summary>
        /// <param name="getPointRequest">Container for the necessary parameters to execute get point request.</param>
        /// <returns>Result of get point request.</returns>
        public Task<GetPointResult> GetPointAsync(GetPointRequest getPointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (getPointRequest == null) throw new ArgumentNullException("getPointRequest");
            return _dynamoDBManager.GetPointAsync(getPointRequest, cancellationToken);
        }

        /// <summary>
        ///     <p>
        ///         Put a point into the Amazon DynamoDB table. Once put, you cannot update attributes specified in
        ///         GeoDataManagerConfiguration: hash key, range key, geohash and geoJson. If you want to update these columns, you
        ///         need to insert a new record and delete the old record.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint geoPoint = new GeoPoint(47.5, -122.3);
        ///         AttributeValue rangeKeyValue = new AttributeValue().withS(&quot;a6feb446-c7f2-4b48-9b3a-0f87744a5047&quot;);
        ///         AttributeValue titleValue = new AttributeValue().withS(&quot;Original title&quot;);
        ///         PutPointRequest putPointRequest = new PutPointRequest(geoPoint, rangeKeyValue);
        ///         putPointRequest.getPutItemRequest().getItem().put(&quot;title&quot;, titleValue);
        ///         PutPointResult putPointResult = geoDataManager.putPoint(putPointRequest);
        ///     </pre>
        /// </summary>
        /// <param name="putPointRequest">Container for the necessary parameters to execute put point request.</param>
        /// <returns>Result of put point request.</returns>
        public Task<PutPointResult> PutPointAsync(PutPointRequest putPointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (putPointRequest == null) throw new ArgumentNullException("putPointRequest");
            return _dynamoDBManager.PutPointAsync(putPointRequest, cancellationToken);
        }

        /// <summary>
        ///     <p>
        ///         Update a point data in Amazon DynamoDB table. You cannot update attributes specified in
        ///         GeoDataManagerConfiguration: hash key, range key, geohash and geoJson. If you want to update these columns, you
        ///         need to insert a new record and delete the old record.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint geoPoint = new GeoPoint(47.5, -122.3);
        ///         String rangeKey = &quot;a6feb446-c7f2-4b48-9b3a-0f87744a5047&quot;;
        ///         AttributeValue rangeKeyValue = new AttributeValue().withS(rangeKey);
        ///         UpdatePointRequest updatePointRequest = new UpdatePointRequest(geoPoint, rangeKeyValue);
        ///         AttributeValue titleValue = new AttributeValue().withS(&quot;Updated title.&quot;);
        ///         AttributeValueUpdate titleValueUpdate = new AttributeValueUpdate().withAction(AttributeAction.PUT)
        ///         .withValue(titleValue);
        ///         updatePointRequest.getUpdateItemRequest().getAttributeUpdates().put(&quot;title&quot;, titleValueUpdate);
        ///         UpdatePointResult updatePointResult = geoIndexManager.updatePoint(updatePointRequest);
        ///     </pre>
        /// </summary>
        /// <param name="updatePointRequest">Container for the necessary parameters to execute update point request.</param>
        /// <returns>Result of update point request.</returns>
        public Task<UpdatePointResult> UpdatePointAsync(UpdatePointRequest updatePointRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dynamoDBManager.UpdatePointAsync(updatePointRequest, cancellationToken);
        }

        /// <summary>
        ///     <p>
        ///         Query a rectangular area constructed by two points and return all points within the area. Two points need to
        ///         construct a rectangle from minimum and maximum latitudes and longitudes. If minPoint.getLongitude() >
        ///         maxPoint.getLongitude(), the rectangle spans the 180 degree longitude line.
        ///     </p>
        ///     <b>Sample usage:</b>
        ///     <pre>
        ///         GeoPoint minPoint = new GeoPoint(45.5, -124.3);
        ///         GeoPoint maxPoint = new GeoPoint(49.5, -120.3);
        ///         QueryRectangleRequest queryRectangleRequest = new QueryRectangleRequest(minPoint, maxPoint);
        ///         QueryRectangleResult queryRectangleResult = geoIndexManager.queryRectangle(queryRectangleRequest);
        ///         for (Map&lt;String, AttributeValue&gt; item : queryRectangleResult.getItem()) {
        ///         System.out.println(&quot;item: &quot; + item);
        ///         }
        ///     </pre>
        /// </summary>
        /// <param name="queryRectangleRequest">Container for the necessary parameters to execute rectangle query request.</param>
        /// <returns>Result of rectangle query request.</returns>
        public async Task<QueryRectangleResult> QueryRectangleAsync(QueryRectangleRequest queryRectangleRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (queryRectangleRequest == null) throw new ArgumentNullException("queryRectangleRequest");
            var latLngRect = S2Util.GetBoundingLatLngRect(queryRectangleRequest);

            var cellUnion = S2Manager.FindCellIds(latLngRect);

            var ranges = MergeCells(cellUnion);

            var result = await DispatchQueries(ranges, queryRectangleRequest, cancellationToken).ConfigureAwait(false);
            return new QueryRectangleResult(result);
        }

        /// <summary>
        ///     Merge continuous cells in cellUnion and return a list of merged GeohashRanges.
        /// </summary>
        /// <param name="cellUnion">Container for multiple cells.</param>
        /// <returns>A list of merged GeohashRanges.</returns>
        private static List<GeohashRange> MergeCells(S2CellUnion cellUnion)
        {
            var ranges = new List<GeohashRange>();
            foreach (var c in cellUnion.CellIds)
            {
                var range = new GeohashRange(c.RangeMin.Id, c.RangeMax.Id);

                var wasMerged = false;
                foreach (var r in ranges)
                {
                    if (r.TryMerge(range))
                    {
                        wasMerged = true;
                        break;
                    }
                }

                if (!wasMerged)
                {
                    ranges.Add(range);
                }
            }

            return ranges;
        }

        /// <summary>
        ///     Filter out any points outside of the queried area from the input list.
        /// </summary>
        /// <param name="list">List of items return by Amazon DynamoDB. It may contains points outside of the actual area queried.</param>
        /// <param name="geoQueryRequest">List of items within the queried area.</param>
        /// <returns></returns>
        private IEnumerable<IDictionary<string, AttributeValue>> Filter(IEnumerable<IDictionary<string, AttributeValue>> list,
                                                                        GeoQueryRequest geoQueryRequest)
        {
            var result = new List<IDictionary<String, AttributeValue>>();

            S2LatLngRect? latLngRect = null;
            S2LatLng? centerLatLng = null;
            double radiusInMeter = 0;
            if (geoQueryRequest is QueryRectangleRequest)
            {
                latLngRect = S2Util.GetBoundingLatLngRect(geoQueryRequest);
            }
            else if (geoQueryRequest is QueryRadiusRequest)
            {
                var centerPoint = ((QueryRadiusRequest)geoQueryRequest).CenterPoint;
                centerLatLng = S2LatLng.FromDegrees(centerPoint.Latitude, centerPoint.Longitude);

                radiusInMeter = ((QueryRadiusRequest)geoQueryRequest).RadiusInMeter;
            }

            foreach (var item in list)
            {
                var geoJson = item[_config.GeoJsonAttributeName].S;
                var geoPoint = GeoJsonMapper.GeoPointFromString(geoJson);

                var latLng = S2LatLng.FromDegrees(geoPoint.Latitude, geoPoint.Longitude);
                if (latLngRect != null && latLngRect.Value.Contains(latLng))
                {
                    result.Add(item);
                }
                else if (centerLatLng != null && radiusInMeter > 0
                         && centerLatLng.Value.GetEarthDistance(latLng) <= radiusInMeter)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private async Task<GeoQueryResult> DispatchQueries(IEnumerable<GeohashRange> ranges, GeoQueryRequest geoQueryRequest, CancellationToken cancellationToken)
        {
            var geoQueryResult = new GeoQueryResult();


            var futureList = new List<Task>();

            var internalSource = new CancellationTokenSource();
            var internalToken = internalSource.Token;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, internalToken);

            
            foreach (var outerRange in ranges)
            {
                foreach (var range in outerRange.TrySplit(_config.HashKeyLength))
                {
                    var task = RunGeoQuery(geoQueryRequest, geoQueryResult, range, cts.Token);
                    futureList.Add(task);
                }
            }

            Exception inner = null;
            try
            {
                for (var i = 0; i < futureList.Count; i++)
                {
                    try
                    {
                        await futureList[i].ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        inner = e;
                        // cancel the others
                        internalSource.Cancel(true);
                    }
                }
            }
            catch (Exception ex)
            {
                inner = inner ?? ex;
                throw new ClientException("Querying Amazon DynamoDB failed.", inner);
            }


            return geoQueryResult;
        }

        private async Task RunGeoQuery(GeoQueryRequest request, GeoQueryResult geoQueryResult, GeohashRange range, CancellationToken cancellationToken)
        {
            var queryRequest = request.QueryRequest.CopyQueryRequest();
            var hashKey = S2Manager.GenerateHashKey(range.RangeMin, _config.HashKeyLength);

            var results = await _dynamoDBManager.QueryGeohashAsync(queryRequest, hashKey, range, cancellationToken).ConfigureAwait(false);

            foreach (var queryResult in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // This is a concurrent collection
                geoQueryResult.QueryResults.Add(queryResult);

                var filteredQueryResult = Filter(queryResult.Items, request);

                // this is a concurrent collection
                foreach (var r in filteredQueryResult)
                    geoQueryResult.Items.Add(r);
            }
        }
    }
}