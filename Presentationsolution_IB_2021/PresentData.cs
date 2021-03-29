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

          


            //string startDateFilter = TableQuery.GenerateFilterCondition(
            // nameof(WeatherEntity.Tid),
            // QueryComparisons.GreaterThanOrEqual, startDate);

            //string endDateFilter = TableQuery.GenerateFilterCondition(
            //nameof(WeatherEntity.Tid),
            //QueryComparisons.LessThanOrEqual, endDate);

            string dates = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition(
             nameof(WeatherEntity.Tid),
             QueryComparisons.GreaterThanOrEqual, startDate), TableOperators.And, TableQuery.GenerateFilterCondition(
             nameof(WeatherEntity.Tid),
            QueryComparisons.LessThanOrEqual, endDate));



            string finalfilter = TableQuery.CombineFilters(dates, TableOperators.And, sourceFilter);




            TableQuery<WeatherEntity> rangeQuery = new TableQuery<WeatherEntity>().Where(finalfilter);
            var segment = weatherdata.ExecuteQuerySegmented(rangeQuery, null);
       
            return new OkObjectResult(segment);

            foreach (WeatherEntity entity in segment) {
                log.LogInformation(
                    $"{entity.PartitionKey}\t{entity.RowKey}\t{entity.Timestamp}\t{entity.Tid}");
            }

        }

    }


}

