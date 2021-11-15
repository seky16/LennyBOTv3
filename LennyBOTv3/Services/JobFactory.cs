using System.Reflection;

namespace LennyBOTv3.Services
{
    public class JobFactory
    {
        private DatabaseService _db;
        private Dictionary<string, MethodInfo> _methods;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public JobFactory(IServiceProvider serviceProvider)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Task.Run(async () =>
            {
                _db = serviceProvider.GetHostedService<DatabaseService>();
                await _db.Initialized;

                _methods = Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes<JobAttribute>().Any() && m.ReturnType == typeof(Task) &&
                        m.IsStatic && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(DateTime))
                    .ToDictionary(m=>m.GetCustomAttribute<JobAttribute>()!.Name);

                var inDb = await _db.GetJobsAsync();
                foreach (var (name, _) in _methods)
                {
                    if (inDb.Any(j => j.Name.Equals(name)))
                        continue;

                    await _db.UpdateJobAsync(new Models.JobModel()
                    {
                        Name = name,
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        LastRunUtc = DateTime.MinValue,
                        RepeatOnError = true,
                    });
                }
            });
        }

        public Task GetJob(string name, DateTime utcNow)
        {
            return (Task)_methods[name].Invoke(null, new object?[] {utcNow})!;
        }

        [Job(nameof(Test))]
        public static async Task Test(DateTime utcNow)
        {
            await Task.Yield();
            //throw new Exception(DateTime.UtcNow.ToString());
            Console.WriteLine(utcNow);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class JobAttribute : Attribute
    {
        public JobAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
