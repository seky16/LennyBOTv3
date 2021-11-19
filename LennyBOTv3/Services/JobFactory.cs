using System.Reflection;
using DSharpPlus.Entities;
using LennyBOTv3.Models;
using SkiaSharp;

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

                var inDb = (await db.GetAllAsync<JobModel>());

                var missingInDb = _methods.Keys.Except(inDb.Select(j => j.Name));

                foreach (var name in missingInDb)
                {
                    await db.UpsertJobAsync(new JobModel()
                    {
                        Name = name,
                        Enabled = true,
                        Interval = TimeSpan.FromMinutes(1),
                        LastRunUtc = DateTime.MinValue,
                        RepeatOnError = true,
                        Running = false,
                    });
                }

                foreach (var job in inDb)
                {
                    if (_methods.ContainsKey(job.Name))
                        await db.UpsertJobAsync(job with { Running = false });
                    else
                        await db.DeleteJobAsync(job.Name);
                }
            });
        }

        public Task GetJob(string name, DateTime utcNow)
        {
            var logger = _loggerFactory.CreateLogger("job " + name);
            logger.LogDebug("{utcNow}", utcNow);
            return (Task)_methods[name].Invoke(null, new object?[] { utcNow, logger, _serviceProvider })!;
        }

        #region Jobs

        [Job(nameof(SendFrogMsg))]
        public static async Task SendFrogMsg(DateTime utcNow, ILogger logger, IServiceProvider serviceProvider)
        {
            var discordClient = serviceProvider.GetHostedService<Bot>().DiscordClient;
            var db = serviceProvider.GetHostedService<DatabaseService>();

            static Stream GetFrogImage(DateTime utcNow)
            {
                var img = SKImage.FromEncodedData("Files/frog.jpg");

                var surf = SKSurface.Create(img.Info);

                var canvas = surf.Canvas;

                canvas.DrawImage(img, 0, 0);

                var paint = new SKPaint()
                {
                    Color = SKColors.White,
                    TextAlign = SKTextAlign.Center,
                    TextSize = 24,
                    Typeface = SKTypeface.FromFamilyName("Times New Roman", SKFontStyle.Normal),
                };

                canvas.DrawText("Gentlemen, it is with great pleasure to inform you that", new SKRect(20, 20, img.Width - 20, 100), paint);
                canvas.DrawText($"today is {utcNow.ToString("dddd, MMMM d", null)}", img.Width / 2, 485, paint);

                var snapshot = surf.Snapshot();
                var data = snapshot.Encode();
                var stream = new MemoryStream();
                data.SaveTo(stream);
                stream.Position = 0;
                return stream;
            }

            var frogs = await db.GetAllAsync<FrogMessageModel>();
            if (frogs.Count == 0)
                return;

            Stream? stream = null;

            var fileName = utcNow.ToString("yyyyMMddhhmm", null) + ".png";

            foreach (var frog in frogs)
            {
                if (utcNow.Subtract(frog.TimeUtc.TimeOfDay).Date <= frog.LastSendUtc)
                    continue;

                var member = await discordClient.GetDiscordMemberAsync(frog.UserId);

                if (member is null)
                    continue;

                if (stream is null)
                    stream = GetFrogImage(utcNow);

                var channel = await member.CreateDmChannelAsync();

                logger.LogInformation("Sending frog msg to {user}", member);
                await channel.SendMessageAsync(new DiscordMessageBuilder().WithFile(fileName, stream));
                await db.UpsertAsync(frog with { LastSendUtc = utcNow.Date });
            }

            stream?.Dispose();
        }

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
