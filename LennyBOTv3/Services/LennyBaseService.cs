namespace LennyBOTv3.Services
{
    public abstract class LennyBaseService<T> where T : LennyBaseService<T>
    {
        private readonly IServiceProvider _serviceProvider;

        public LennyBaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected DatabaseService Database => _serviceProvider.GetHostedService<DatabaseService>();
        protected ILogger<T> Logger => _serviceProvider.GetRequiredService<ILogger<T>>();
    }
}
