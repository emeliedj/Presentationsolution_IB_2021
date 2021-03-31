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
using System.Linq;

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
            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. Läs API-dokumentationen för hjälp.");
        }



        [FunctionName("GetWeatherBySource")]
        public static IActionResult GetWeatherStation(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/source/{source}")] HttpRequest req,
                   [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source)
        {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, source.ToLower());

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. Läs API-dokumentationen för hjälp.");
        }


        [FunctionName("GetWeatherByDate")]
        public static IActionResult GetWeatherByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}")] HttpRequest req,
                [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source, string startDate, string endDate)
        {
            
                string sourceFilter = TableQuery.GenerateFilterCondition(
                 nameof(WeatherEntity.PartitionKey),
                 QueryComparisons.Equal, source.ToLower());

                string startDateFilter = TableQuery.GenerateFilterCondition(
                 nameof(WeatherEntity.Tid),
                 QueryComparisons.GreaterThanOrEqual, startDate);

                string endDateFilter = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.Tid),
                QueryComparisons.LessThanOrEqual, endDate);


                string finalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(sourceFilter, TableOperators.And, startDateFilter), TableOperators.And, endDateFilter);


                TableQuery<WeatherEntity> rangeQuery = new TableQuery<WeatherEntity>().Where(finalfilter);
                var segment = weatherdata.ExecuteQuerySegmented(rangeQuery, null);


            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. Läs API-dokumentationen för hjälp.");

        }

        [FunctionName("GetWeatherByType")]
        public static async Task<IActionResult> GetWeatherByType(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}/typ/{typ}")] HttpRequest req,
           [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata,
           ILogger log, string source, string startDate, string endDate, string typ)
        {

            string sourceFilter = TableQuery.GenerateFilterCondition(
            nameof(WeatherEntity.PartitionKey),
            QueryComparisons.Equal, source.ToLower());

            string startDateFilter = TableQuery.GenerateFilterCondition(
             nameof(WeatherEntity.Tid),
             QueryComparisons.GreaterThanOrEqual, startDate);

            string endDateFilter = TableQuery.GenerateFilterCondition(
            nameof(WeatherEntity.Tid),
            QueryComparisons.LessThanOrEqual, endDate);

            string finalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(sourceFilter, TableOperators.And, startDateFilter), TableOperators.And, endDateFilter);


            if (typ.ToLower().Equals("Nederbörd".ToLower()))
            {
                TableQuery<WeatherNederbörd> projectionQuery = new TableQuery<WeatherNederbörd>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Nederbörd" });
                var weatherDatas = await weatherdata.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return new OkObjectResult(weatherDatas);

            } else if (typ.ToLower().Equals("Vindstyrka".ToLower()))
            {
                TableQuery<WeatherVindstyrka> projectionQuery = new TableQuery<WeatherVindstyrka>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Vindstyrka" });
                var weatherDatas = await weatherdata.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return new OkObjectResult(weatherDatas);
            } else if (typ.ToLower().Equals("Grad".ToLower()))
            {
                TableQuery<WeatherGrad> projectionQuery = new TableQuery<WeatherGrad>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Grad" });
                var weatherDatas = await weatherdata.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return new OkObjectResult(weatherDatas);
            } else {
                return new BadRequestObjectResult("Attans, kontrollera din inmatning. Läs API-dokumentationen för hjälp.");
            }



        }


    }


}

