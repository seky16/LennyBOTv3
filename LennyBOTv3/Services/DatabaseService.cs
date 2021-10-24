using System.Text;
using DSharpPlus.Entities;
using LiteDB;
using Newtonsoft.Json.Linq;

namespace LennyBOTv3.Services
{
    public sealed class DatabaseService : BackgroundService
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private LiteDatabase _db;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _db = new("LennyBOTv3.db");
            _db.Mapper.EnumAsInteger = true;
            _db.UtcDate = true;
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

        public Task<string?> GetUserLocationAsync(DiscordUser user)
            => Task.Run(() => _db.GetCollection("UserLocations").FindById(user.Id)?["location"].AsString);

        public Task SetUserLocationAsync(DiscordUser user, string location)
            => Task.Run(() => _db.GetCollection("UserLocations").Upsert(user.Id, new() { ["user"] = $"{user.Username}#{user.Discriminator}", ["location"] = location, }));
    }
}
