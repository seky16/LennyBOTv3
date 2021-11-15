using System.Text;
using DSharpPlus.Entities;
using LennyBOTv3.Models;
using LiteDB;

namespace LennyBOTv3.Services
{
    public sealed class DatabaseService : BackgroundService
    {
        private readonly LiteDatabase _db;
        public Task Initialized => _init.Task;

        private readonly TaskCompletionSource _init = new();

        public DatabaseService()
        {
            _db = new("LennyBOTv3.db");
            _db.Mapper.EnumAsInteger = true;
            _db.UtcDate = true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _init.SetResult();
            await Task.Delay(-1, stoppingToken);
        }

        public override void Dispose()
        {
            _db.Dispose();
            base.Dispose();
        }

        public Task<string> RunQuery(string query)
        {
            return Task.Run(() =>
            {
                var reader = _db.Execute(query);
                var sb = new StringBuilder();

                foreach (var item in reader.ToEnumerable())
                {
                    using var strWriter = new StringWriter(sb);
                    var jsonWriter = new JsonWriter(strWriter) { Pretty = true, };
                    jsonWriter.Serialize(item);
                }
                return sb.ToString();
            });
        }

        #region Settings

        public Task<string?> GetUserLocationAsync(DiscordUser user)
            => Task.Run(() => _db.GetCollection("UserLocations").FindById(user.Id)?["location"].AsString);

        public Task SetUserLocationAsync(DiscordUser user, string location)
            => Task.Run(() => _db.GetCollection("UserLocations").Upsert(user.Id, new() { ["user"] = $"{user.Username}#{user.Discriminator}", ["location"] = location, }));

        #endregion Settings

        #region RSS

        public Task<List<RssFeedModel>> GetRssFeedsAsync(ulong channelId)
            => Task.Run(() =>
            {
                var feeds = _db.GetCollection<RssFeedModel>();
                feeds.EnsureIndex(f => f.ChannelId);
                var results = feeds.Query().Where(f => f.ChannelId == channelId);
                return results.ToList();
            });

        public Task RemoveRssFeed(DiscordChannel channel, string name)
            => Task.Run(() =>
            {
                var feeds = _db.GetCollection<RssFeedModel>();
                feeds.EnsureIndex(f => f.ChannelId);
                feeds.DeleteMany(f => f.ChannelId == channel.Id && f.Name == name);
            });

        public Task AddRssFeed(RssFeedModel rssFeedModel)
            => Task.Run(() => _db.GetCollection<RssFeedModel>().Insert(rssFeedModel));

        #endregion RSS

        #region Jobs

        public Task<List<JobModel>> GetJobsAsync()
            => Task.Run(() => _db.GetCollection<JobModel>().FindAll().ToList());

        public Task UpdateJob(JobModel jobModel)
            => Task.Run(() => _db.GetCollection<JobModel>().Upsert(jobModel));

        #endregion Jobs
    }
}
