using DSharpPlus;
using DSharpPlus.Entities;
using LennyBOTv3.Models;

namespace LennyBOTv3.Services
{
    public class RssService : LennyBaseService<RssService>
    {
        public RssService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task AddFeed(Uri url, DiscordChannel channel, string name)
            => Database.AddRssFeed(new RssFeedModel() { Name = name, ChannelId = channel.Id, Url = url.AbsoluteUri, LastUpdatedUtc = DateTime.UtcNow });

        public async Task<DiscordEmbed> ListFeeds(DiscordChannel channel)
        {
            var feeds = await Database.GetRssFeedsAsync(channel.Id);
            if (!feeds.Any())
                return new DiscordEmbedBuilder().WithTitle($"No feeds for {channel}");

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Feeds for {channel}");
            foreach (var feed in feeds)
            {
                embed.AddField(feed.Name, $"{feed.Url}{Environment.NewLine}Last updated: {Formatter.Timestamp(feed.LastUpdatedUtc)}");
            }
            return embed;
        }

        public Task RemoveFeed(DiscordChannel channel, string name)
                    => Database.RemoveRssFeed(channel, name);
    }
}
