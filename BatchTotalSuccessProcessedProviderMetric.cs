using LogProducer.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Flurl.Http;

namespace LogProducer.Provider
{
    public class BatchTotalSuccessProcessedProviderMetric : IProviderMetric
    {
        public async Task SendMetricData(ILogger logger, string message)
        {
            var metricDataValue = new Metric();
            metricDataValue.series = new List<MetricUnit>();

            var random = new Random();
            var rvalue = random.Next(0, 600);

            var munit = new MetricUnit
            {
                host = "www.asb.co.nz",
                metric = "asb_payment_batch_total_success_processed",
                type = "count",
                interval = 20,
                points = new List<long[]>() {
                    new long[] 
                    { 
                        DateTime.Now.ToUnixTime(), 
                        Convert.ToInt64(rvalue) 
                    }
                },
                tags = message
            };

            metricDataValue.series.Add(munit);
            string postMetricData = JsonConvert.SerializeObject(metricDataValue);

            var response = await AppConstant.DATADOG_METRIC_SERIES_POST_URL.PostJsonAsync(metricDataValue);
            logger.LogInformation($"Provider: {this.GetType().Name}): {postMetricData}, response: {response}");
        }
    }
}
