using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Geo;
using Amazon.Geo.Model;
using Amazon.Runtime;
using Newtonsoft.Json;
using SampleServer.Models;

namespace SampleServer.Controllers
{
    public class SearchController : Controller
    {
        private GeoDataManager _geoDataManager;
        private GeoDataManagerConfiguration _config;

        public SearchController()
        {
            SetupGeoDataManager();
        }

        private void SetupGeoDataManager()
        {
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

            _config= new GeoDataManagerConfiguration(ddb, tableName);
            _geoDataManager = new GeoDataManager(_config);

        }


        //
        // GET: /Search/
        public ActionResult Index()
        {
            return View();
        }

        
        public async Task<ActionResult> QueryRadius(RadiusQuery query)
        {
            if (!ModelState.IsValid)
                return Json("Bad Request", JsonRequestBehavior.AllowGet);

            var centerPoint = new GeoPoint(query.Lat, query.Lng);
            var radius = query.RadiusInMeters;

            var attributesToGet = new List<string>
            {
                _config.RangeKeyAttributeName,
                _config.GeoJsonAttributeName,
                "schoolName"
            };

            var radReq = new QueryRadiusRequest(centerPoint, radius);
            radReq.QueryRequest.AttributesToGet = attributesToGet;

            var result = await _geoDataManager.QueryRadiusAsync(radReq);

            var dtos = GetResultsFromQuery(result);
            

            return Json(dtos, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> QueryRectangle(RectangleQuery query)
        {
            if (!ModelState.IsValid)
                return Json("{\"result\":\"Bad Request\"}", JsonRequestBehavior.AllowGet);

            var min = new GeoPoint(query.MinLat, query.MinLng);
            var max = new GeoPoint(query.MaxLat, query.MaxLng);
            

            var attributesToGet = new List<string>
            {
                _config.RangeKeyAttributeName,
                _config.GeoJsonAttributeName,
                "schoolName"
            };

            var radReq = new QueryRectangleRequest(min, max);
            radReq.QueryRequest.AttributesToGet = attributesToGet;

            var result = await _geoDataManager.QueryRectangleAsync(radReq);

            var dtos = GetResultsFromQuery(result);


            return Json(dtos, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SchoolSearchResult> GetResultsFromQuery(GeoQueryResult result)
        {
            var dtos = from item in result.Items
                       let geoJsonString = item[_config.GeoJsonAttributeName].S
                       let point = JsonConvert.DeserializeObject<GeoPoint>(geoJsonString)
                       select new SchoolSearchResult
                       {
                           Latitude = point.Latitude,
                           Longitude = point.Longitude,
                           RangeKey = item[_config.RangeKeyAttributeName].S,
                           SchoolName = item.ContainsKey("schoolName") ? item["schoolName"].S : string.Empty
                       };

            return dtos;
        }

        
    }
}