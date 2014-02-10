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
    public class GeoDataManager
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
        public async Task<QueryRadiusResult> QueryRadiusAsync(QueryRadiusRequest queryRadiusRequest)
        {
            var latLngRect = S2Util.GetBoundingLatLngRect(queryRadiusRequest);

            var cellUnion = S2Manager.FindCellIds(latLngRect);

            var ranges = MergeCells(cellUnion);
            cellUnion = null;

            var result = await DispatchQueries(ranges, queryRadiusRequest).ConfigureAwait(false);
            return new QueryRadiusResult(result);
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
        public async Task<QueryRectangleResult> QueryRectangleAsync(QueryRectangleRequest queryRectangleRequest)
        {
            var latLngRect = S2Util.GetBoundingLatLngRect(queryRectangleRequest);

            var cellUnion = S2Manager.FindCellIds(latLngRect);

            var ranges = MergeCells(cellUnion);

            var result = await DispatchQueries(ranges, queryRectangleRequest).ConfigureAwait(false);
            return new QueryRectangleResult(result);
        }

        /// <summary>
        ///     Merge continuous cells in cellUnion and return a list of merged GeohashRanges.
        /// </summary>
        /// <param name="cellUnion">Container for multiple cells.</param>
        /// <returns>A list of merged GeohashRanges.</returns>
        private List<GeohashRange> MergeCells(S2CellUnion cellUnion)
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

            S2LatLngRect latLngRect;
            S2LatLng centerLatLng;
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
                if (latLngRect != default(S2LatLngRect) && latLngRect.Contains(latLng))
                {
                    result.Add(item);
                }
                else if (centerLatLng != default(S2LatLng) && radiusInMeter > 0
                         && centerLatLng.GetEarthDistance(latLng) <= radiusInMeter)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private async Task<GeoQueryResult> DispatchQueries(IEnumerable<GeohashRange> ranges, GeoQueryRequest geoQueryRequest)
        {
            var geoQueryResult = new GeoQueryResult();


            var futureList = new List<Task>();

            var cts = new CancellationTokenSource();

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
                        cts.Cancel(true);
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

            var results = await _dynamoDBManager.QueryGeohashAsync(queryRequest, hashKey, range).ConfigureAwait(false);

            foreach (var queryResult in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // This is a concurrent collection
                geoQueryResult.QueryResults.Add(queryResult);

                var filteredQueryResult = Filter(queryResult.Items, request);

                // this is a concurrent collection
                foreach (var r in filteredQueryResult)
                    geoQueryResult.Item.Add(r);
            }
        }
    }
}