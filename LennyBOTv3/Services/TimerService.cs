using Nito.AsyncEx;

namespace LennyBOTv3.Services
{
    public class TimerService : LennyBackgroundService<TimerService>
    {
        private readonly JobFactory _jobFactory;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _dispatcher;

        public TimerService(IServiceProvider serviceProvider, JobFactory jobFactory) : base(serviceProvider)
        {
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _serviceProvider.GetHostedService<Bot>().Initialized;
            Init.SetResult();
            _dispatcher = new(Tick, stoppingToken, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private async void Tick(object? o)
        {
            var token = o as CancellationToken? ?? CancellationToken.None;
            var utcNow = DateTime.UtcNow;
            var jobs = await Database.GetJobsAsync(utcNow);
            var tasks = jobs.ToDictionary(j => _jobFactory.GetJob(j.Name, utcNow));
            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks.Keys);
                var job = tasks[t];
                tasks.Remove(t);
                var enabled = true;
                try { await t; }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Job '{name}' threw exception", job.Name);
                    if (!job.RepeatOnError)
                    {
                        enabled = false;
                        Logger.LogInformation("Disabling job '{jobName}'", job.Name);
                    }
                }
                await Database.UpsertJobAsync(job with { Running = false, Enabled = enabled });
            }
        }
    }
}
