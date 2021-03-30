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
using System.Collections.Generic;

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
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/{partitionKey}")] HttpRequest req,
                   [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source)
        {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, source);

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }


        [FunctionName("GetWeatherDate")]
        public static IActionResult GetWeatherByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/datum/{datum}")] HttpRequest req,
                [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source, string startDate, string endDate)
        {


            string sourceFilter = TableQuery.GenerateFilterCondition(
             nameof(WeatherEntity.PartitionKey),
             QueryComparisons.Equal, source);



            string startDateFilter = TableQuery.GenerateFilterCondition(
             nameof(WeatherEntity.Tid),
             QueryComparisons.GreaterThanOrEqual, startDate);

            string endDateFilter = TableQuery.GenerateFilterCondition(
            nameof(WeatherEntity.Tid),
            QueryComparisons.LessThanOrEqual, endDate);



            string finalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(sourceFilter, TableOperators.And, startDateFilter), TableOperators.And, endDateFilter);




            TableQuery<WeatherEntity> rangeQuery = new TableQuery<WeatherEntity>().Where(finalfilter);
            var segment = weatherdata.ExecuteQuerySegmented(rangeQuery, null);


            return new OkObjectResult(segment);
        }

        [FunctionName("GetAllWeather")]
        public static async Task<IActionResult> GetWeatherByType(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}/typ/{typ}")] HttpRequest req,
           [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata,
           ILogger log, string source, string startDate, string endDate, string typ)
        {
            

            TableQuery<WeatherSorted> projectionQuery = new TableQuery<WeatherSorted>().Select(
              new string[] { "PartitionKey", "Tid" });

            var weatherDatas = await weatherdata.ExecuteQuerySegmentedAsync(projectionQuery, null);
            List<WeatherSorted> weather = new List<WeatherSorted>();
            foreach (var c in weatherDatas.Results)
            {
                weather.Add(new WeatherSorted
                {
                    Tid = c.Tid,
                    PartitionKey = c.PartitionKey,
                    Nederbörd = c.Nederbörd,

                });

            }

            return weather != null
                ? (ActionResult)new OkObjectResult(weather)
                : new BadRequestObjectResult("Nothing.");


        }




    }


}

