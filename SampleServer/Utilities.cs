using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Geo;
using Amazon.Geo.Model;
using Amazon.Geo.Util;
using Amazon.Runtime;

namespace SampleServer
{
    public class Utilities
    {
        private Utilities()
        {
            Status = Status.NotStarted;
            
        }

        public static readonly Utilities Instance = new Utilities();


        private GeoDataManager _geoDataManager;
        public Status Status { get; private set; }

        public bool IsAccessKeySet
        {
            get
            {
                var key = ConfigurationManager.AppSettings["AWS_ACCESS_KEY_ID"];
                return !string.IsNullOrWhiteSpace(key);
            }
        }

        public bool IsSecretKeySet
        {
            get
            {
                var key = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                return !string.IsNullOrWhiteSpace(key);
            }
        }

        public bool IsTableNameSet
        {
            get
            {
                var key = ConfigurationManager.AppSettings["PARAM1"];
                return !string.IsNullOrWhiteSpace(key);
            }
        }

        public bool IsRegionNameSet
        {
            get
            {
                var key = ConfigurationManager.AppSettings["PARAM2"];
                return !string.IsNullOrWhiteSpace(key);
            }
        }

        public async Task SetupTable()
        {
            if (!(IsAccessKeySet && IsSecretKeySet && IsRegionNameSet && IsTableNameSet))
                throw new InvalidOperationException("Not configured yet.");

            SetupGeoDataManager();

            var config = _geoDataManager.GeoDataManagerConfiguration;
            var dtr = new DescribeTableRequest()
            {
                TableName = config.TableName
            };

            Task t = Task.FromResult(false);
            try
            {
                await config.DynamoDBClient.DescribeTableAsync(dtr);
                if (Status == Status.NotStarted)
                    Status = Status.Ready;
            }
            catch (ResourceNotFoundException e)
            {
                t = StartLoadData();
            }
            await t;

        }

        private void SetupGeoDataManager()
        {
            if(_geoDataManager == null)
            lock (this)
            {
                if (_geoDataManager != null)
                    return;

            

                var accessKey = ConfigurationManager.AppSettings["AWS_ACCESS_KEY_ID"];
                var secretKey = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                var tableName = ConfigurationManager.AppSettings["PARAM1"];
                var regionName = ConfigurationManager.AppSettings["PARAM2"];


                var region = RegionEndpoint.GetBySystemName(regionName);
                var config = new AmazonDynamoDBConfig {MaxErrorRetry = 20, RegionEndpoint = region};
                var creds = new BasicAWSCredentials(accessKey, secretKey);


                bool useLocalDb;
                bool.TryParse(ConfigurationManager.AppSettings["AWS_USE_LOCAL_DB"], out useLocalDb);

                if (useLocalDb)
                {
                    var localDB = ConfigurationManager.AppSettings["AWS_LOCAL_DYNAMO_DB_ENDPOINT"];
                    config.ServiceURL = localDB;
                }

                var ddb = new AmazonDynamoDBClient(creds, config);

                var gConfig = new GeoDataManagerConfiguration(ddb, tableName);
                _geoDataManager = new GeoDataManager(gConfig);
            }
        }

        private async Task StartLoadData()
        {
            Status = Status.CreatingTable;

            var config = _geoDataManager.GeoDataManagerConfiguration;

            var ctr = GeoTableUtil.GetCreateTableRequest(config);
            await config.DynamoDBClient.CreateTableAsync(ctr);

            await WaitForTableToBeReady();

            await InsertData();
        }

        private async Task InsertData()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\school_list_wa.txt");

            foreach (var line in File.ReadLines(path))
            {
                var columns = line.Split('\t');
                var schoolId = columns[0];
                var schoolName = columns[1];
                var latitude = double.Parse(columns[2], CultureInfo.InvariantCulture);
                var longitude = double.Parse(columns[3], CultureInfo.InvariantCulture);

                var point = new GeoPoint(latitude, longitude);

                var rangeKeyVal = new AttributeValue {S = schoolId};
                var schoolNameVal = new AttributeValue {S = schoolName};

                var req = new PutPointRequest(point, rangeKeyVal);
                req.PutItemRequest.Item["schoolName"] = schoolNameVal;

                await _geoDataManager.PutPointAsync(req);
            }

            Status = Status.Ready;
        }


        private async Task WaitForTableToBeReady()
        {
            var config = _geoDataManager.GeoDataManagerConfiguration;
            var dtr = new DescribeTableRequest {TableName = config.TableName};
            var result = await config.DynamoDBClient.DescribeTableAsync(dtr);

            while (result.Table.TableStatus != TableStatus.ACTIVE)
            {
                await Task.Delay(2000);
                result = await config.DynamoDBClient.DescribeTableAsync(dtr);
            }
        }

    }

    public enum Status { NotStarted, CreatingTable, InsertingDataToTable, Ready}
}