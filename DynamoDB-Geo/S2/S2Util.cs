using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Geo.Model;
using Google.Common.Geometry;

namespace Amazon.Geo.S2
{
    internal static class S2Util
    {


        /// <summary>
        /// An utility method to get a bounding box of latitude and longitude from a given GeoQueryRequest.
        /// </summary>
        /// <param name="geoQueryRequest">It contains all of the necessary information to form a latitude and longitude box.</param>
        /// <returns></returns>
        public static S2LatLngRect GetBoundingLatLngRect(GeoQueryRequest geoQueryRequest)
        {
            if (geoQueryRequest is QueryRectangleRequest)
            {
                var queryRectangleRequest = (QueryRectangleRequest)geoQueryRequest;

                var minPoint = queryRectangleRequest.MinPoint;
                var maxPoint = queryRectangleRequest.MaxPoint;

                var latLngRect = default(S2LatLngRect);

                if (minPoint != null && maxPoint != null)
                {
                    var minLatLng = S2LatLng.FromDegrees(minPoint.Latitude, minPoint.Longitude);
                    var maxLatLng = S2LatLng.FromDegrees(maxPoint.Latitude, maxPoint.Longitude);

                    latLngRect = new S2LatLngRect(minLatLng, maxLatLng);
                }

                return latLngRect;
            }
            else if (geoQueryRequest is QueryRadiusRequest)
            {
                var queryRadiusRequest = (QueryRadiusRequest)geoQueryRequest;

                var centerPoint = queryRadiusRequest.CenterPoint;
                var radiusInMeter = queryRadiusRequest.RadiusInMeter;

                var centerLatLng = S2LatLng.FromDegrees(centerPoint.Latitude, centerPoint.Longitude);

                var latReferenceUnit = centerPoint.Latitude > 0.0 ? -1.0 : 1.0;
                var latReferenceLatLng = S2LatLng.FromDegrees(centerPoint.Latitude + latReferenceUnit,
                                                              centerPoint.Longitude);
                var lngReferenceUnit = centerPoint.Longitude > 0.0 ? -1.0 : 1.0;
                var lngReferenceLatLng = S2LatLng.FromDegrees(centerPoint.Latitude, centerPoint.Longitude
                                                                                    + lngReferenceUnit);

                var latForRadius = radiusInMeter/centerLatLng.GetEarthDistance(latReferenceLatLng);
                var lngForRadius = radiusInMeter/centerLatLng.GetEarthDistance(lngReferenceLatLng);

                var minLatLng = S2LatLng.FromDegrees(centerPoint.Latitude - latForRadius,
                                                     centerPoint.Longitude - lngForRadius);
                var maxLatLng = S2LatLng.FromDegrees(centerPoint.Latitude + latForRadius,
                                                     centerPoint.Longitude + lngForRadius);

                return new S2LatLngRect(minLatLng, maxLatLng);
            }

            return S2LatLngRect.Empty;
        }
    }
}