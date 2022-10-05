using nsPocOrlns.Infrastructure.EventStreaming;

namespace nsPocOrlns.KafkaWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEventHub _eventHub;

        public Worker(ILogger<Worker> logger, IEventHub eventHub)
        {
            _logger = logger;
            _eventHub = eventHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}