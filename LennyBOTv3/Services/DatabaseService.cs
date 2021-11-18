using System.Text;
using DSharpPlus.Entities;
using LennyBOTv3.Models;
using LiteDB;

namespace LennyBOTv3.Services
{
    public sealed class DatabaseService : LennyBackgroundService<DatabaseService>
    {
        private readonly LiteDatabase _db;

        public DatabaseService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _db = new("Files/LennyBOTv3.db");
            _db.Mapper.EnumAsInteger = true;
            _db.UtcDate = true;
        }

        public override void Dispose()
        {
            _db.Dispose();
            base.Dispose();
        }

        public Task<List<T>> GetAllAsync<T>()
            => Task.Run(() => _db.GetCollection<T>().FindAll().ToList());

        public Task<string> RunQueryAsync(string query)
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

        public Task UpsertAsync<T>(T model)
                    => Task.Run(() => _db.GetCollection<T>().Upsert(model));

        #region Settings

        public Task<string?> GetUserLocationAsync(DiscordUser user)
            => Task.Run(() => _db.GetCollection("UserLocations").FindById(user.Id)?["location"].AsString);

        public Task SetUserLocationAsync(DiscordUser user, string location)
            => Task.Run(() => _db.GetCollection("UserLocations").Upsert(user.Id, new() { ["user"] = $"{user.Username}#{user.Discriminator}", ["location"] = location, }));

        #endregion Settings

        #region RSS

        public Task AddRssFeed(RssFeedModel rssFeedModel)
            => Task.Run(() => _db.GetCollection<RssFeedModel>().Insert(rssFeedModel));

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

        #endregion RSS

        #region Jobs

        public Task DeleteJobAsync(string name)
            => Task.Run(() => _db.GetCollection<JobModel>().Delete(name));

        public Task<List<JobModel>> GetJobsAsync(DateTime utcNow)
            => Task.Run(async () =>
            {
                var jobs = (await GetAllAsync<JobModel>()).Where(j => j.Enabled && !j.Running && j.LastRunUtc.Add(j.Interval) <= utcNow);
                var newJobs = jobs.Select(j => j with { LastRunUtc = utcNow, Running = true }).ToList();
                var upserts = newJobs.Select(j => UpsertJobAsync(j));
                await Task.WhenAll(upserts);

                return newJobs;
            });

        public Task UpsertJobAsync(JobModel jobModel)
            => Task.Run(() => _db.GetCollection<JobModel>().Upsert(jobModel));

        #endregion Jobs
    }
}
