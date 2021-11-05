using Nito.AsyncEx;

namespace LennyBOTv3.Services
{
    public class TimerService : BackgroundService
    {
        private readonly JobFactory _jobFactory;
        private readonly AsyncLock _lock = new();
        private readonly ILogger<TimerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _dispatcher;

        public TimerService(ILogger<TimerService> logger, IServiceProvider serviceProvider, JobFactory jobFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _jobFactory = jobFactory;
        }

        private DatabaseService Database => _serviceProvider.GetHostedService<DatabaseService>();

        public override void Dispose()
        {
            _dispatcher?.Change(Timeout.Infinite, Timeout.Infinite);
            _dispatcher?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _dispatcher = new(Tick, stoppingToken, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        private async void Tick(object? o)
        {
            var token = o as CancellationToken? ?? CancellationToken.None;
            using (await _lock.LockAsync(token))
            {
                var utcNow = DateTime.UtcNow;
                var jobs = await Database.GetJobsAsync();
                foreach (var job in jobs)
                {
                    _logger.LogTrace("{job}", job);
                    if (job.Enabled && job.LastRunUtc.Add(job.Interval) <= utcNow)
                    {
                        var enabled = true;
                        _logger.LogDebug("Running job '{name}'", job.Name);
                        var jobTask = _jobFactory.GetJob(job.Name);
                        try
                        {
                            await jobTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Job '{name}' threw exception", job.Name);
                            if (!job.RepeatOnError)
                                enabled = false;
                        }
                        await Database.UpdateJob(job with { LastRunUtc = utcNow, Enabled = enabled });
                    }
                }
            }
        }
    }
}
