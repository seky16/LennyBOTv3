using System.Reflection;
using LennyBOTv3.Models;

namespace LennyBOTv3.Services
{
    public class JobFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, MethodInfo> _methods;

        public JobFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            Task.Run(async () =>
            {
                var db = serviceProvider.GetHostedService<DatabaseService>();
                await db.Initialized;

                _methods = Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes<JobAttribute>().Any() && m.ReturnType == typeof(Task)
                        && m.IsStatic && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType == typeof(DateTime)
                        && m.GetParameters()[1].ParameterType == typeof(ILogger) && m.GetParameters()[2].ParameterType == typeof(IServiceProvider))
                    .ToDictionary(m => m.GetCustomAttribute<JobAttribute>()!.Name);

                var inDb = (await db.GetJobsAsync()).Select(j => j.Name);

                var missingInDb = _methods.Keys.Except(inDb);
                var missingInCode = inDb.Except(_methods.Keys);

                foreach (var name in missingInDb)
                {
                    await db.UpsertJobAsync(new Models.JobModel()
                    {
                        Name = name,
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        LastRunUtc = DateTime.MinValue,
                        RepeatOnError = true,
                    });
                }

                foreach (var name in missingInCode)
                {
                    await db.DeleteJobAsync(name);
                }
            });
        }

        public Task GetJob(string name, DateTime utcNow)
        {
            var logger = _loggerFactory.CreateLogger("job " + name);
            return (Task)_methods[name].Invoke(null, new object?[] { utcNow, logger, _serviceProvider })!;
        }

        #region Jobs

        //[Job(nameof(Test))]
        public static async Task Test(DateTime utcNow, ILogger logger, IServiceProvider serviceProvider)
        {
            //throw new Exception(DateTime.UtcNow.ToString());
            logger.LogInformation("{utcNow}", utcNow);
        }

        [Job(nameof(UpdateChannelTopic))]
        public static async Task UpdateChannelTopic(DateTime utcNow, ILogger logger, IServiceProvider serviceProvider)
        {
            var discordClient = serviceProvider.GetHostedService<Bot>().DiscordClient;
            var db = serviceProvider.GetHostedService<DatabaseService>();

            var descriptions = await db.GetAllAsync<ChannelDescriptionModel>();
            foreach (var desc in descriptions)
            {
                var channel = await discordClient.GetChannelAsync(desc.ChannelId);

                if (channel is null)
                    continue;

                var topic = desc.GetTopic(utcNow);

                if (string.Equals(channel.Topic, topic))
                    continue;

                logger.LogInformation("Changing {channel} topic to '{topic}'", channel, topic);
                await channel.ModifyAsync(c => c.Topic = topic);
            }
        }

        #endregion
    }

    #region JobAttribute

    [AttributeUsage(AttributeTargets.Method)]
    public class JobAttribute : Attribute
    {
        public JobAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    #endregion
}
