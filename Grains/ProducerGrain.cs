using System;
using System.Threading.Tasks;
using Common;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;

namespace Grains
{
    public class ProducerGrain : Grain, IProducerGrain
    {
        private readonly ILogger<IProducerGrain> logger;

        private IAsyncStream<int> stream;

        public ProducerGrain(ILogger<IProducerGrain> logger)
        {
            this.logger = logger;
        }

        public async Task StartProducing(string ns, Guid key)
        {
            // Get the stream
            this.stream = base
                .GetStreamProvider(Constants.StreamProvider)
                .GetStream<int>(key, ns);

            // Register a timer that produce an event every second
            await this.stream.OnNextAsync(1);
            this.logger.LogInformation("Produced event");
        }

        public Task StopProducing()
        {
            return Task.CompletedTask;
        }
    }
}
