﻿using System.Text;
using DSharpPlus;
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

        public Task<List<DiscordEmbedBuilder>> RunQueryAsync(string query)
        {
            return Task.Run(() =>
            {
                var reader = _db.Execute(query);
                var result = new List<DiscordEmbedBuilder>();
                var list = reader.ToList();
                var vals = list.OfType<BsonDocument>().SelectMany(d => d.Values).OfType<BsonArray>().Where(a => a.Any()).SelectMany(x => x);
                if (!vals.Any())
                    vals = list;

                if (!vals.Any())
                    return new() { new DiscordEmbedBuilder().WithTitle("No results") };
                var count = vals.Count();
                for (var i = 0; i < count; i++)
                {
                    var item = vals.ElementAt(i);
                    var sb = new StringBuilder();
                    using var strWriter = new StringWriter(sb);
                    var jsonWriter = new JsonWriter(strWriter) { Pretty = true, };
                    jsonWriter.Serialize(item);
                    result.Add(new DiscordEmbedBuilder().WithTitle($"Result {i + 1}/{count}").WithDescription(Formatter.BlockCode(sb.ToString(), "json")));
                }
                return result;
            });
        }

        public Task UpdateAsync<T>(T model)
            => Task.Run(() => { if (!_db.GetCollection<T>().Update(model)) Logger.LogWarning("{method}: {model} not found", nameof(UpdateAsync), model); });

        public Task UpsertAsync<T>(T model)
            => Task.Run(() => { if (_db.GetCollection<T>().Upsert(model)) Logger.LogDebug("Inserting {model}", model); });

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
