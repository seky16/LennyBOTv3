using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using LennyBOTv3.Services;

namespace LennyBOTv3.Modules
{
    public class SearchModule : LennyBaseModule
    {
        public SearchService SearchService { private get; set; }

        [Command("imdb")]
        [Description("")]
        public async Task ImdbAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await SearchService.ImdbAsync(query));

        [Command("urban")]
        [Description("")]
        public async Task UrbanDictionaryAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await SearchService.UrbanDictionaryAsync(ctx, query));

        [Command("weather")]
        [Description("")]
        public async Task WeatherAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await SearchService.WeatherAsync(query));

        [Command("wiki"), Aliases("wikipedia")]
        [Description("")]
        public async Task WikipediaAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await SearchService.WikipediaAsync(query));

        [Command("yt"), Aliases("youtube")]
        [Description("")]
        public async Task YouTubeAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.RespondAsync(await SearchService.YouTubeAsync(query));
    }
}
