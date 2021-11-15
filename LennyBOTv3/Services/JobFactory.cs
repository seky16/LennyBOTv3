namespace LennyBOTv3.Services
{
    public class JobFactory
    {
        private DatabaseService _db;

        public JobFactory(IServiceProvider serviceProvider)
        {
            Task.Run(async () =>
            {
                _db = serviceProvider.GetHostedService<DatabaseService>();
                await _db.Initialized;

                // todo
                // get all [Job]s
                // cache them (dict [name]=[methodInfo?])
                // add missing to db
                // use cache in getJob
            });
        }

        public Task GetJob(string name)
        {
            switch (name)
            {
                case nameof(test):
                    return test();

                default:
                    throw new NotImplementedException(name);
            }
        }

        private async Task test()
        {
            await Task.Yield();
            //throw new Exception(DateTime.UtcNow.ToString());
            Console.WriteLine(DateTime.UtcNow);
        }
    }
}
