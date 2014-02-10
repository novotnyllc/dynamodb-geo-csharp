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
        /**
	 * An utility method to get a bounding box of latitude and longitude from a given GeoQueryRequest.
	 * 
	 * @param geoQueryRequest
	 *            It contains all of the necessary information to form a latitude and longitude box.
	 * 
	 * */
        public static S2LatLngRect GetBoundingLatLngRect(GeoQueryRequest geoQueryRequest) {
		if (geoQueryRequest is QueryRectangleRequest) {
			QueryRectangleRequest queryRectangleRequest = (QueryRectangleRequest) geoQueryRequest;

			GeoPoint minPoint = queryRectangleRequest.MinPoint;
			GeoPoint maxPoint = queryRectangleRequest.MaxPoint;

			S2LatLngRect latLngRect = default(S2LatLngRect);

			if (minPoint != null && maxPoint != null) {
				S2LatLng minLatLng = S2LatLng.FromDegrees(minPoint.Latitude, minPoint.Longitude);
				S2LatLng maxLatLng = S2LatLng.FromDegrees(maxPoint.Latitude, maxPoint.Longitude);

				latLngRect = new S2LatLngRect(minLatLng, maxLatLng);
			}

			return latLngRect;
		} else if (geoQueryRequest is QueryRadiusRequest) {
			QueryRadiusRequest queryRadiusRequest = (QueryRadiusRequest) geoQueryRequest;

			GeoPoint centerPoint = queryRadiusRequest.CenterPoint;
			double radiusInMeter = queryRadiusRequest.RadiusInMeter;

			S2LatLng centerLatLng = S2LatLng.FromDegrees(centerPoint.Latitude, centerPoint.Longitude);

			double latReferenceUnit = centerPoint.Latitude > 0.0 ? -1.0 : 1.0;
			S2LatLng latReferenceLatLng = S2LatLng.FromDegrees(centerPoint.Latitude + latReferenceUnit,
					centerPoint.Longitude);
			double lngReferenceUnit = centerPoint.Longitude > 0.0 ? -1.0 : 1.0;
			S2LatLng lngReferenceLatLng = S2LatLng.FromDegrees(centerPoint.Latitude, centerPoint.Longitude
					+ lngReferenceUnit);

			double latForRadius = radiusInMeter / centerLatLng.GetEarthDistance(latReferenceLatLng);
            double lngForRadius = radiusInMeter / centerLatLng.GetEarthDistance(lngReferenceLatLng);

			S2LatLng minLatLng = S2LatLng.FromDegrees(centerPoint.Latitude - latForRadius,
					centerPoint.Longitude - lngForRadius);
			S2LatLng maxLatLng = S2LatLng.FromDegrees(centerPoint.Latitude + latForRadius,
					centerPoint.Longitude + lngForRadius);

			return new S2LatLngRect(minLatLng, maxLatLng);
		}

        return S2LatLngRect.Empty;
	}
    }
}
