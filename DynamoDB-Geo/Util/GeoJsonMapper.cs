using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Geo.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Amazon.Geo.Util
{
    public static class GeoJsonMapper
    {
        internal static readonly JsonSerializerSettings JsonSerializerSettings;

        static GeoJsonMapper()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public static GeoPoint GeoPointFromString(string jsonString)
        {
            return JsonConvert.DeserializeObject<GeoPoint>(jsonString, JsonSerializerSettings);
        }

        public static string StringFromGeoObject(GeoObject geoObject)
        {
            return JsonConvert.SerializeObject(geoObject, JsonSerializerSettings);
        }
    }
}
