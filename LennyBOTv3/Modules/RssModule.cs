using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LennyBOTv3.Services;

namespace LennyBOTv3.Modules
{
    [Group("rss")]
    [Description("")]
    public class RssModule : LennyBaseModule
    {
        private readonly RssService _rss;

        public RssModule(RssService rss)
        {
            _rss = rss;
        }

        [Command("add")]
        [Description("")]
        public async Task RssAddAsync(CommandContext ctx,
            [Description("")] Uri url,
            [Description("")] DiscordChannel? channel = null,
            [RemainingText, Description("")] string? name = null)
        {
            await _rss.AddFeed(url, channel ?? ctx.Channel, name ?? url.Host);
            await ctx.MarkSuccessAsync();
        }

        [Command("list")]
        [Description("")]
        public async Task RssListAsync(CommandContext ctx,
            [RemainingText, Description("")] DiscordChannel? channel = null)
            => await ctx.RespondAsync(await _rss.ListFeeds(channel ?? ctx.Channel));

        [Command("remove"), Priority(1)]
        [Description("")]
        public async Task RssRemoveAsync(CommandContext ctx,
            [Description("")] DiscordChannel channel,
            [RemainingText, Description("")] string name)
        {
            await _rss.RemoveFeed(channel, name);
            await ctx.MarkSuccessAsync();
        }

        [Command("remove")]
        [Description("")]
        public Task RssRemoveAsync(CommandContext ctx,
            [RemainingText, Description("")] string name)
            => RssRemoveAsync(ctx, ctx.Channel, name);
    }
}
