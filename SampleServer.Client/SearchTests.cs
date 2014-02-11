using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace SampleServer.Client
{
    [TestClass]
    public class SearchTests
    {
        private HttpClient _client;
        
        public SearchTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(ConfigurationManager.AppSettings["testHost"])
            };
        }
        [TestMethod]
        public async Task QueryRadiusTest()
        {
            const double latitude = 47.61121;
            const double longitude = -122.31846;
            const double radiusInMeters = 10000;

            var url = string.Format(CultureInfo.InvariantCulture, "search/QueryRadius?lat={0}&lng={1}&radiusInMeters={2}", latitude, longitude, radiusInMeters);

            var result = await _client.GetStringAsync(url);

            var arr = JsonConvert.DeserializeObject<SchoolSearchResult[]>(result);

            // For this search with the populated data, we should get 324 results

            Assert.AreEqual(324, arr.Length);
        }
    }
}
