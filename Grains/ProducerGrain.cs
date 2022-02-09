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
        private IDisposable timer;

        private int counter = 0;

        public ProducerGrain(ILogger<IProducerGrain> logger)
        {
            this.logger = logger;
        }

        public async Task StartProducing(string ns, Guid key)
        {
            if (this.timer != null)
                throw new Exception("This grain is already producing events");

            // Get the stream
            this.stream = base
                .GetStreamProvider(Constants.StreamProvider)
                .GetStream<int>(key, ns);

            // Register a timer that produce an event every second
            await this.stream.OnNextAsync(1);
            this.logger.LogInformation("Produced event");
        }

        private async Task TimerTick(object _)
        {
            var value = counter++;
            this.logger.LogInformation("Sending event {EventNumber}", value);
            await this.stream.OnNextAsync(value);
        }

        public Task StopProducing()
        {
            if (this.stream != null)
            {
                this.timer.Dispose();
                this.timer = null;
                this.stream = null;
            }

            return Task.CompletedTask;
        }
    }
}
