using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Flurl.Http;
using LogProducer.Model;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using LogProducer.Provider;

namespace LogProducer
{
    public class Functions
    {
        public static string ApiKey = "75cb6c3734c6525a829f8e0ca18ceaac";

        private static List<IProviderMetric> providers = new List<IProviderMetric>
        {
            new BatchApprovalWaitingOnProviderMetric(),
            new BatchAverageTimeProviderMetric(),
            new BatchCompletedProviderMetric(),
            new BatchReceivedProviderMetric(),
            new BatchStatusProviderMetric(),
            new BatchTotalProcessedProviderMetric(),
            new BatchTotalReturnsProviderMetric(),
            new BatchTotalSuccessProcessedProviderMetric(),
            new BatchCompletedProviderMetric()
        };


        public static async Task ProcessQueueMessage([QueueTrigger("queue")] string message, ILogger logger)
        {
            logger.LogInformation($"Getting data from queue : {message}, {DateTime.Now}");
            
            //var response = await $"https://api.datadoghq.com/api/v1/validate?api_key={ApiKey}".GetStringAsync();
            //logger.LogInformation(response);

            foreach (var provider in providers)
            {
                await provider.SendMetricData(logger, message);
            }

            // await SendMetricData(logger, message);
            //await SendMetricDataClient(logger, message);

            logger.LogInformation($"Done.");
        }

        public static async Task SendKeyValidation(ILogger logger)
        {
            var response = await $"https://api.datadoghq.com/api/v1/validate?api_key={ApiKey}".GetStringAsync();
            logger.LogInformation($"SendKeyValidation : {response}");
        }

        #region DevelopmentCode
        public static async Task SendMetricData(ILogger logger, string message)
        {
            try
            {
                var seriesRootData = new Metric();
                seriesRootData.series = new List<MetricUnit>();

                var random = new Random();
                var rvalue = random.Next(0, 600);

                var munit = new MetricUnit
                {
                    host = "www.asb.co.nz",
                    metric = "asb_payment_batch_demo",
                    type = "count",
                    interval = 20,

                    //points = new List<Point[]>() { 
                    //    new Point[] { new Point(DateTime.Now.ToUnixTime(), 320)}, 
                    //},

                    points = new List<long[]>() {
                    new long[] { DateTime.Now.ToUnixTime(), Convert.ToInt64(rvalue) }
                },
                    tags = message
                };

                seriesRootData.series.Add(munit);
                string output = JsonConvert.SerializeObject(seriesRootData);

                //var data = "{ \"series\" : [{\"metric\":\"asb_payment_batch_demo\", \"points\":[[1571611333, 555]], \"type\":\"rate\", \"interval\": 20, \"host\":\"test.example.com\", \"tags\":[\"environment:test\"]}]}";

                logger.LogInformation($"Output from json (unserialized data:retest): {output}");

                var response = await "https://api.datadoghq.com/api/v1/series?api_key=75cb6c3734c6525a829f8e0ca18ceaac".PostJsonAsync(seriesRootData);

                // logger.LogInformation($"AppDataDogMetricData:v1: {response.StatusCode} : {response}");
            }
            catch (Exception ex)
            {
                var totalmessage = ex?.Message + "\n" + ex?.InnerException?.Message + "\n" + ex?.StackTrace;
                logger.LogInformation($"Error.v6 :  {totalmessage} ");
            }
        }

        public static async Task SendMetricDataClient(ILogger logger, string message)
        {
            try
            {
                var seriesRootData = new Metric();
                seriesRootData.series = new List<MetricUnit>();

                var random = new Random();
                var rvalue = random.Next(0, 600);

                var munit = new MetricUnit
                {
                    host = "www.asb.co.nz",
                    metric = "asb_payment_batch_demo",
                    type = "rate",
                    interval = 20,
                    points = new List<long[]>() {
                    new long[] { DateTime.Now.ToUnixTime(), Convert.ToInt64(rvalue) }
                },

                    tags = message
                };

                seriesRootData.series.Add(munit);
                string output = JsonConvert.SerializeObject(seriesRootData);

                var Url = "https://api.datadoghq.com/api/v1/series?api_key=75cb6c3734c6525a829f8e0ca18ceaac";

                logger.LogInformation($"Output from json (httpclient v1): {output}");

                HttpContent httpContent = new StringContent(output, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var result = await client.PostAsync(Url, httpContent);

                    HttpContent responseContent = result.Content;
                    // Get the stream of the content.
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        // Write the output.
                        logger.LogInformation("Final response output" + await reader.ReadToEndAsync());
                    }


                }
            }
            catch (Exception ex)
            {
                var totalmessage = ex?.Message + "\n" + ex?.InnerException?.Message + "\n" + ex?.StackTrace;
                logger.LogInformation($"Error.v10x :  {totalmessage} ");
            }
        }


        #region Logs 
        public static async Task SendLogData(ILogger logger, string message)
        {
            var customLog = new SriniApplicationLog
            {
                Id = Guid.NewGuid().ToString(),
                Details = $"Application status is {DateTime.Now.ToString()} : {message}",
                //Severity = "info"
                Severity = "error"
                // LogType = "ERR1026"
            };

            string output = JsonConvert.SerializeObject(customLog);
            logger.LogInformation($"Output from json : {output}");
            var response = await $"https://http-intake.logs.datadoghq.com/v1/input/{ApiKey}".PostJsonAsync(output);
            logger.LogInformation($"AppDataLogv3: {response.StatusCode} : {response}");
        }
        #endregion
    } 
    #endregion
}


