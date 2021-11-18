namespace LennyBOTv3.Services
{
    public abstract class LennyBackgroundService<T> : BackgroundService where T : LennyBackgroundService<T>
    {
        protected readonly TaskCompletionSource Init = new();

        protected LennyBackgroundService(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetRequiredService<ILogger<T>>();
        }

        public Task Initialized => Init.Task;

        protected ILogger<T> Logger { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init.SetResult();
            await Task.Delay(-1, stoppingToken);
        }
    }
}
