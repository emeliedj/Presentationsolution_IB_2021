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

            //string exist = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition(WeatherEntity.Substring(0,9), QueryComparisons.Equal, startDate), TableOperators.And, TableQuery.GenerateFilterCondition(nameof(WeatherEntity.Tid).Substring(0, 9), QueryComparisons.Equal, endDate));




            //string startDateFilter = TableQuery.GenerateFilterCondition(
            // nameof(WeatherEntity.Tid),
            // QueryComparisons.GreaterThanOrEqual, startDate);

            //string endDateFilter = TableQuery.GenerateFilterCondition(
            //nameof(WeatherEntity.Tid),
            //QueryComparisons.LessThanOrEqual, endDate);

            string dates = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition(
             "Tid",
             QueryComparisons.GreaterThanOrEqual, startDate), TableOperators.And, TableQuery.GenerateFilterCondition(
            "Tid",
            QueryComparisons.LessThanOrEqual, endDate));



            //string dateFilter = TableQuery.CombineFilters(dates, TableOperators.And, source);

            string finalfilter = TableQuery.CombineFilters(dates, TableOperators.And, sourceFilter);




            TableQuery<WeatherEntity> rangeQuery = new TableQuery<WeatherEntity>().Where(finalfilter);
            var segment = weatherdata.ExecuteQuerySegmented(rangeQuery, null);
       
            return new OkObjectResult(segment);

            foreach (WeatherEntity entity in segment) {
                Console.WriteLine("Weather: {0}, {1}, {2}, {3}", entity.PartitionKey, entity.Grad, entity.Tid, entity.Vindstyrka, entity.Nederb�rd);
            }
           
        }

    }


}

