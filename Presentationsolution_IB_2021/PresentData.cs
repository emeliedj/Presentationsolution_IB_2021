using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs.Host;

namespace Presentationsolution_IB_2021
{
    public static class PresentData
    {
        [FunctionName("GetAllWeather")]
        public static async Task<IActionResult> GetWeather(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather")] HttpRequest req,
            [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata,
            ILogger log)
        {
            var query = new TableQuery<WeatherEntity>();
            var segment = await weatherdata.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment);
        }
       
    

     [FunctionName("GetWeatherSource")]
        public static IActionResult GetWeatherStation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/partitionKey/{partitionKey}")] HttpRequest req,
                [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string partitionKey)
        {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, partitionKey);

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }

        [FunctionName("GetGrad")]
        public static IActionResult GetGrad(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/datum/{datum}")] HttpRequest req,
                [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string datum)
        {

            string tid = nameof(WeatherEntity.Tid);
            string datumet = tid.Substring(0, 9);

            var filterQuery = TableQuery.GenerateFilterCondition(
                datumet,
                QueryComparisons.Equal, datum);

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }

    }


}

