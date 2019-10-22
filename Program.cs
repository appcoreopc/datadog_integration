


using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var builder = new HostBuilder();
            
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
            });


            builder.ConfigureLogging((context, b) =>
            {
                b.AddConsole();

                string instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (!string.IsNullOrEmpty(instrumentationKey))
                {
                    b.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = instrumentationKey);
                }
            });


            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
            });

            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
    }
}
