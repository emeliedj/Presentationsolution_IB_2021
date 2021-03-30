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



        [FunctionName("GetWeatherBySource")]
        public static IActionResult GetWeatherStation(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/source/{source}")] HttpRequest req,
                   [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata, ILogger log, string source)
        {

            var filterQuery = TableQuery.GenerateFilterCondition(
                nameof(WeatherEntity.PartitionKey),
                QueryComparisons.Equal, source);

            var query = new TableQuery<WeatherEntity>().Where(filterQuery);
            var segment = weatherdata.ExecuteQuerySegmented(query, null);
            return new OkObjectResult(segment);
        }


        [FunctionName("GetWeatherByDate")]
        public static IActionResult GetWeatherByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}")] HttpRequest req,
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

        [FunctionName("GetWeatherByType")]
        public static async Task<IActionResult> GetWeatherByType(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather/source/{source}/startDate/{startDate}/endDate/{endDate}/typ/{typ}")] HttpRequest req,
           [Table("weatherdata", Connection = "AzureWebJobsStorage")] CloudTable weatherdata,
           ILogger log, string source, string startDate, string endDate, string typ)
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



      
            TableQuery<WeatherEntity> projectionQuery = new TableQuery<WeatherEntity>().Where(finalfilter).Select(
              new string[] { "PartitionKey", "Tid", typ });

            var weatherDatas = await weatherdata.ExecuteQuerySegmentedAsync(projectionQuery, null);
            List<WeatherNederbörd> weatherNederbörd = new List<WeatherNederbörd>();
            List<WeatherGrad> weatherGrad = new List<WeatherGrad>();
            List<WeatherVindstyrka> weatherVind = new List<WeatherVindstyrka>();


            foreach (var c in weatherDatas.Results)
            {
                if (typ.Equals("Nederbörd"))
                {
                    weatherNederbörd.Add(new WeatherNederbörd
                    {
                        Tid = c.Tid,
                        PartitionKey = c.PartitionKey,
                        Nederbörd = c.Nederbörd
                    });
                    return weatherNederbörd != null
                ? (ActionResult)new OkObjectResult(weatherNederbörd)
                : new BadRequestObjectResult("Attans");
                }
                else if (typ.Equals("Vindstyrka"))
                    {
                    weatherVind.Add(new WeatherVindstyrka
                    {
                        Tid = c.Tid,
                        PartitionKey = c.PartitionKey,
                        Vindstyrka = c.Vindstyrka
                    });
                    return weatherVind != null
                  ? (ActionResult)new OkObjectResult(weatherVind)
                     : new BadRequestObjectResult("Attans");

                } else
                {
                    weatherGrad.Add(new WeatherGrad
                    {
                        Tid = c.Tid,
                        PartitionKey = c.PartitionKey,
                        Grad = c.Grad

                    });
                    return weatherGrad != null
                  ? (ActionResult)new OkObjectResult(weatherGrad)
                     : new BadRequestObjectResult("Attans");
                }

            }

            return weatherGrad != null
                     ? (ActionResult)new OkObjectResult(weatherGrad)
                     : new BadRequestObjectResult("Attans");

        }


    }


}

