using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LennyBOTv3.Services;

namespace LennyBOTv3.Modules
{
    public class SearchModule : LennyBaseModule
    {
        private readonly SearchService _searchService;

        public SearchModule(SearchService searchService)
        {
            _searchService = searchService;
        }

        [Command("imdb")]
        [Description("")]
        public async Task ImdbAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await _searchService.ImdbAsync(query));

        [Command("urban")]
        [Description("")]
        public async Task UrbanDictionaryAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await _searchService.UrbanDictionaryAsync(ctx, query));

        [Command("weather")]
        [Description("")]
        public async Task WeatherAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await _searchService.WeatherAsync(query));

        [Command("weather"), Priority(1)]
        [Description("")]
        public async Task WeatherAsync(CommandContext ctx,
            [RemainingText, Description("")] DiscordUser? user = null)
            => await ctx.SendPaginatedMessageAsync(await _searchService.WeatherAsync(user ?? ctx.User));

        [Command("wiki"), Aliases("wikipedia")]
        [Description("")]
        public async Task WikipediaAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await _searchService.WikipediaAsync(query));

        [Command("yt"), Aliases("youtube")]
        [Description("")]
        public async Task YouTubeAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.RespondAsync(await _searchService.YouTubeAsync(query));
    }
}
