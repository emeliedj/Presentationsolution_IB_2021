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
    public static class PresentData {
        [FunctionName("GetAllWeather")]
        public static async Task<IActionResult> GetWeather(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather")] HttpRequest req,
            [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata,
            ILogger log) {
            var query = new TableQuery<WeatherEntity>();
            var segment = await weatherdata.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment);
        }



        [FunctionName("GetWeatherSource")]
        public static IActionResult GetWeatherStation(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/{partitionKey}")] HttpRequest req,
                   [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string partitionKey) {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, partitionKey);

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }


        [FunctionName("GetWeatherByDate")]
        public static IActionResult GetWeatherDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}")] HttpRequest req,
                [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source, string startDate, string endDate)
        {

           
            string sourceFilter = TableQuery.GenerateFilterCondition(
              nameof(WeatherEntity.PartitionKey),
              QueryComparisons.Equal, source);




           string tid = nameof(WeatherEntity.Tid).Substring(0, 9);

           string startDateCheck = TableQuery.GenerateFilterCondition(
           tid,
           QueryComparisons.Equal, startDate);

           string endDateCheck = TableQuery.GenerateFilterCondition(
           tid,
           QueryComparisons.Equal, endDate);


            string exist = TableQuery.CombineFilters(startDateCheck, TableOperators.And, endDateCheck);

           
            
            string startDateFilter = TableQuery.GenerateFilterCondition(
            nameof(WeatherEntity.Tid),
            QueryComparisons.GreaterThanOrEqual, startDate);

            string endDateFilter = TableQuery.GenerateFilterCondition(
            nameof(WeatherEntity.Tid),
            QueryComparisons.LessThanOrEqual, endDate);




            string controlFilter = TableQuery.CombineFilters(exist, TableOperators.And, sourceFilter);

            string datefilter = TableQuery.CombineFilters(startDateFilter, TableOperators.And, endDateFilter);

            string finalFilter = TableQuery.CombineFilters(datefilter, TableOperators.And, controlFilter);




            var query = new TableQuery<WeatherEntity>().Where(finalFilter);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }

    }


}

