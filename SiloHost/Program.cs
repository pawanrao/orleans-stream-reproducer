using System;
using System.Threading.Tasks;
using Common;
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
                    .UseOrleans(ConfigureSilo)
                    .ConfigureLogging(logging =>
                    {
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
            var secrets = Secrets.LoadFromFile();
            siloBuilder
                .AddMemoryGrainStorage("PubSubStore")
                .AddSqsStreams(Constants.StreamProvider, options =>
                {
                    options.ConfigurePartitioning(1);
                    options.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
                    options.ConfigureSqs(ob => ob.Configure(sqsOptions =>
                    {
                        sqsOptions.ConnectionString = "Service=us-east-1";
                    }));
                    options.ConfigurePullingAgent(ob => ob.Configure(pOps =>
                      {
                          pOps.BatchContainerBatchSize = 2;
                      }));
                })
                .UseLocalhostClustering(serviceId: Constants.ServiceId, clusterId: Constants.ServiceId);
        }
    }
}
