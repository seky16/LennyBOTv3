using System.ServiceModel.Syndication;
using System.Xml;
using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using LennyBOTv3.Models;

namespace LennyBOTv3.Services
{
    public class RssService : LennyBaseService<RssService>
    {
        public RssService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task AddFeed(Uri url, DiscordChannel channel, string name)
                    => Database.AddRssFeed(new RssFeedModel() { Name = name, ChannelId = channel.Id, Url = url.AbsoluteUri, LastUpdatedUtc = null, Enabled = true, LastItemId = null });

        public async Task<DiscordEmbed> ListFeeds(DiscordChannel channel)
        {
            var feeds = await Database.GetRssFeedsAsync(channel.Id);
            if (feeds.Count == 0)
                return new DiscordEmbedBuilder().WithTitle($"No feeds for {channel}");

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Feeds for {channel}");
            foreach (var feed in feeds)
            {
                embed.AddField(feed.Name + (feed.Enabled ? "" : " (disabled)"), $"{feed.Url}{Environment.NewLine}Last updated: {(feed.LastUpdatedUtc.HasValue ? Formatter.Timestamp(feed.LastUpdatedUtc.Value) : "N/A")}");
            }
            return embed;
        }

        public Task RemoveFeed(DiscordChannel channel, string name)
                    => Database.RemoveRssFeed(channel, name);

        #region CheckRssFeeds

        [Job(nameof(CheckRssFeeds))]
        public static async Task CheckRssFeeds(DateTime utcNow, ILogger logger, IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetHostedService<Bot>().DiscordClient;
            var db = serviceProvider.GetHostedService<DatabaseService>();

            var feeds = (await db.GetAllAsync<RssFeedModel>()).Where(f => f.Enabled);
            using var http = new HttpClient();
            var tasks = new List<Task>();

            foreach (var (url, channelFeeds) in feeds.GroupBy(f => f.Url))
            {
                var rss = await LoadFeed(http, url, db, logger);
                if (rss is null)
                    continue;

                foreach (var feed in channelFeeds)
                {
                    var items = rss.Items.ToList(); // counting on correct ordering of items
                    if (items.Count == 0)
                        continue;
                    if (feed.LastUpdatedUtc?.Equals(rss.LastUpdatedTime.UtcDateTime) ?? false)
                        continue;
                    if (feed.LastItemId?.Equals(items[0].Id) ?? false)
                        continue;

                    var channel = await client.GetChannelAsync(feed.ChannelId);
                    var count = 0;
                    foreach (var item in items)
                    {
                        if (item.Id.Equals(feed.LastItemId))
                            break;

                        var (content, embed) = GetPost(feed, item);
                        tasks.Add(channel.SendMessageAsync(content, embed));
                        count++;
                    }

                    if (count > 0)
                        logger.LogInformation("Posting {count} items from {feed} to {channel}", count, url, channel);

                    var lastUpdatedUtc = rss.LastUpdatedTime == default(DateTimeOffset) ? (DateTime?)null : rss.LastUpdatedTime.UtcDateTime;
                    var newFeed = feed with { LastUpdatedUtc = lastUpdatedUtc, LastItemId = items[0].Id };
                    logger.LogDebug("Updating feed: {feed}", newFeed);
                    await db.UpdateAsync(newFeed);
                }
            }

            await Task.WhenAll(tasks);
        }

        private static (string? Content, DiscordEmbedBuilder? Embed) GetPost(RssFeedModel feed, SyndicationItem item)
        {
            var selfUrl = item.Links.FirstOrDefault(x => string.Equals(x.RelationshipType, "self", StringComparison.InvariantCultureIgnoreCase))?.Uri;

            if (selfUrl is not null)
            {
                return (selfUrl.OriginalString, null);
            }
            else
            {
                var postUrl = item.Links.FirstOrDefault(x => string.Equals(x.RelationshipType, "alternate", StringComparison.InvariantCultureIgnoreCase))?.Uri
                    ?? item.Links.FirstOrDefault(x => x.Uri != null)?.Uri;
                var conv = new CustomConverter();
                var summary = conv.Convert(item.Summary?.Text ?? string.Empty);
                var content = conv.Convert((item.Content as TextSyndicationContent)?.Text ?? string.Empty);
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(item.Title.Text)
                    .WithUrl(postUrl)
                    .WithDescription((!string.IsNullOrWhiteSpace(summary) ? summary : content).Truncate(1024))
                    .WithTimestamp(item.GetTimestampUtc())
                    .WithAuthor(feed.Name, feed.Url)
                    .WithImageUrl(conv.Images.FirstOrDefault());

                return (null, embed);
            }
        }

        private static async Task<SyndicationFeed?> LoadFeed(HttpClient http, string url, DatabaseService db, ILogger logger)
        {
            try
            {
                using var get = await http.GetAsync(url);
                using var res = await get.Content.ReadAsStreamAsync();
                using var xml = XmlReader.Create(res);
                return SyndicationFeed.Load(xml);
            }
            catch (Exception ex)
            {
                //todo
                logger.LogDebug(ex, "{url}", url);
                return null;
            }
        }

        public class CustomConverter : ReverseMarkdown.Converter
        {
            public CustomConverter() : base(new() { RemoveComments = true, SmartHrefHandling = false, })
            {
                Register("img", new CustomImgConverter(this));
            }

            public List<string> Images { get; } = new();
        }

        public class CustomImgConverter : ReverseMarkdown.Converters.ConverterBase
        {
            public CustomImgConverter(CustomConverter converter) : base(converter)
            {
            }

            public override string Convert(HtmlNode node)
            {
                var alt = node.GetAttributeValue("alt", string.Empty);
                var src = node.GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrWhiteSpace(src))
                    ((CustomConverter)Converter).Images.Add(src);

                return alt;// string.Empty;
            }
        }

        #endregion CheckRssFeeds
    }
}
