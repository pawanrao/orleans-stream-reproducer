using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Streams;

namespace SiloHost
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var host = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    })
                    .ConfigureHostConfiguration(config =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    })
                    .UseOrleans(ConfigureSilo)
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole(c =>
                        {
                            c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                            c.IncludeScopes = true;
                        });
                    })
                    .Build();

                await host.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void ConfigureSilo(ISiloBuilder siloBuilder)
        {
            siloBuilder
                .AddMemoryGrainStorage("PubSubStore")
                .AddAzureQueueStreams(Constants.StreamProvider, options =>
                {
                    options.ConfigureAzureQueue(ob => ob.Configure(aqOptions =>
                    {
                        aqOptions.QueueNames = new List<string> { "test" };
                        aqOptions.ConnectionString = "";
                    }));
                    options.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
                    options.ConfigurePullingAgent(ob=>ob.Configure(ops =>
                    {
                        ops.BatchContainerBatchSize = 2;
                    }));

                })
                .UseLocalhostClustering(serviceId: Constants.ServiceId, clusterId: Constants.ServiceId);
        }
    }
}
