using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace Amazon.Geo
{
    public class GeoDataManagerConfiguration
    {
        // Public constants
        public const long MergeThreshold = 2;

        // Default values
        private const string DefaultHashkeyAttributeName = "hashKey";
        private const string DefaultRangekeyAttributeName = "rangeKey";
        private const string DefaultGeohashAttributeName = "geohash";
        private const string DefaultGeojsonAttributeName = "geoJson";
        private const string DefaultGeohashIndexAttributeName = "geohash-index";

        private const int DefaultHashkeyLength = 6;

        // Configuration properties


        public GeoDataManagerConfiguration(AmazonDynamoDBClient dynamoDBClient, String tableName)
        {
            HashKeyAttributeName = DefaultHashkeyAttributeName;
            RangeKeyAttributeName = DefaultRangekeyAttributeName;
            GeohashAttributeName = DefaultGeohashAttributeName;
            GeoJsonAttributeName = DefaultGeojsonAttributeName;

            GeohashIndexName = DefaultGeohashIndexAttributeName;

            HashKeyLength = DefaultHashkeyLength;

            DynamoDBClient = dynamoDBClient;
            TableName = tableName;
        }

        public string TableName { get; set; }

        public string HashKeyAttributeName { get; set; }


        public string RangeKeyAttributeName { get; set; }


        public string GeohashAttributeName { get; set; }


        public string GeoJsonAttributeName { get; set; }


        public string GeohashIndexName { get; set; }


        public int HashKeyLength { get; set; }


        public AmazonDynamoDBClient DynamoDBClient { get; set; }
    }
}