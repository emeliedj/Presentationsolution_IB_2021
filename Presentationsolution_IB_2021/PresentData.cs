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
            [Table("weatherdatastab", Connection = "AzureWebJobsStorage")] CloudTable weatherdatastab,
            ILogger log)
        {
            var query = new TableQuery<WeatherEntity>();
            var segment = await weatherdatastab.ExecuteQuerySegmentedAsync(query, null);
            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp");
        }



        [FunctionName("GetWeatherBySource")]
        public static IActionResult GetWeatherStation(
               [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}")] HttpRequest req,
                   [Table("weatherdatastab", Connection = "AzureWebJobsStorage")] CloudTable weatherdatastab, ILogger log, string source)
        {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, source.ToLower());

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdatastab.ExecuteQuerySegmented(query, null);
            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");
        }


        [FunctionName("GetWeatherByDate")]
        public static IActionResult GetWeatherByDate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}")] HttpRequest req,
                [Table("weatherdatastab", Connection = "AzureWebJobsStorage")] CloudTable weatherdatastab, ILogger log, string source, string startDate, string endDate)
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
                var segment = weatherdatastab.ExecuteQuerySegmented(rangeQuery, null);


            return segment.Results.Any() ? new OkObjectResult(segment) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");

        }

        [FunctionName("GetWeatherByType")]
        public static async Task<IActionResult> GetWeatherByType(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}/weatherType/{weatherType}")] HttpRequest req,
           [Table("weatherdatastab", Connection = "AzureWebJobsStorage")] CloudTable weatherdatastab,
           ILogger log, string source, string startDate, string endDate, string weatherType)
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


            if (weatherType.ToLower().Equals("Nederb?rd".ToLower()))
            {
                TableQuery<WeatherNederb?rd> projectionQuery = new TableQuery<WeatherNederb?rd>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Nederb?rd" });
                var weatherDatas = await weatherdatastab.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return weatherDatas.Results.Any() ? new OkObjectResult(weatherDatas) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");

            } else if (weatherType.ToLower().Equals("Vindstyrka".ToLower()))
            {
                TableQuery<WeatherVindstyrka> projectionQuery = new TableQuery<WeatherVindstyrka>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Vindstyrka" });
                var weatherDatas = await weatherdatastab.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return weatherDatas.Results.Any() ? new OkObjectResult(weatherDatas) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");
              
            } else if (weatherType.ToLower().Equals("Grad".ToLower()))
            {
                TableQuery<WeatherGrad> projectionQuery = new TableQuery<WeatherGrad>().Where(finalfilter).Select(
                new string[] { "PartitionKey", "Tid", "Grad" });
                var weatherDatas = await weatherdatastab.ExecuteQuerySegmentedAsync(projectionQuery, null);
                return weatherDatas.Results.Any() ? new OkObjectResult(weatherDatas) : new OkObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");
            } else {
                return new BadRequestObjectResult("Attans, kontrollera din inmatning. L?s API-dokumentationen f?r hj?lp.");
            }



        }


    }


}

