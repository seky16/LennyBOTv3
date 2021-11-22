using LennyBOTv3.Models;

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
            _dispatcher = new(Tick, stoppingToken, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            await base.ExecuteAsync(stoppingToken);
        }

        private async void Tick(object? o)
        {
            var utcNow = DateTime.UtcNow;
            var jobs = await Database.GetJobsAsync(utcNow);

            var taskToJob = new Dictionary<int, JobModel>();
            var tasks = new List<Task>();
            foreach (var job in jobs)
            {
                var task = _jobFactory.GetJob(job.Name, utcNow);
                taskToJob[task.Id] = job;
                tasks.Add(task);
            }

            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                var job = taskToJob[t.Id];
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
