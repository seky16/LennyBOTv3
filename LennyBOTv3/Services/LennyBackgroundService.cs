using System.Diagnostics.CodeAnalysis;

namespace LennyBOTv3.Services
{
    public abstract class LennyBackgroundService<T> : BackgroundService where T : LennyBackgroundService<T>
    {
        protected readonly TaskCompletionSource Init = new();
        private readonly Task? _dbTask;

        protected LennyBackgroundService(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetRequiredService<ILogger<T>>();
            ServiceProvider = serviceProvider;
            if (this is DatabaseService db)
            {
                Database = db;
            }
            else
            {
                _dbTask = Task.Run(async () =>
               {
                   Database = serviceProvider.GetHostedService<DatabaseService>();
                   await Database.Initialized;
               });
            }
        }

        public Task Initialized => Init.Task;

        [NotNull]
        protected DatabaseService? Database { get; private set; }

        protected ILogger<T> Logger { get; }
        protected IServiceProvider ServiceProvider { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_dbTask is not null)
                await _dbTask;

            Init.SetResult();
            await Task.Delay(-1, stoppingToken);
        }
    }
}
