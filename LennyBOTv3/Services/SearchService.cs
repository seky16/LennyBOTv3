﻿using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Google.Apis.YouTube.v3;
using LennyBOTv3.Models;
using LennyBOTv3.Settings;
using Microsoft.Extensions.Options;
using OMDbApiNet;

namespace LennyBOTv3.Services
{
    public class SearchService
    {
        private readonly AsyncOmdbClient _omdb;
        private readonly YouTubeService _youTube;

        public SearchService(AsyncOmdbClient omdb,YouTubeService youTube)
        {
            _omdb = omdb;
            _youTube = youTube;
        }

        internal async Task<IEnumerable<DiscordEmbedBuilder>> ImdbAsync(string query)
        {
            var list = await _omdb.GetSearchListAsync(query);
            if (!string.IsNullOrEmpty(list.Error))
                throw new HttpRequestException($"OmdbApi returned an error: {list.Error}");

            if (list.SearchResults.Count == 0)
                throw new HttpRequestException("OmdbApi did not return any result");

            var pages = new List<DiscordEmbedBuilder>();
            foreach (var result in list.SearchResults)
            {
                var item = await _omdb.GetItemByIdAsync(result.ImdbId);
                pages.Add(new DiscordEmbedBuilder()
                    .WithAuthor("IMDb", "https://www.imdb.com/", "http://files.softicons.com/download/social-media-icons/flat-gradient-social-icons-by-guilherme-lima/png/512x512/IMDb.png")
                    .WithColor(DiscordColor.Goldenrod)
                    .WithTitle($"{item.Title} ({item.Year})")
                    .WithDescription($"{item.Plot} ({item.Runtime})")
                    .WithUrl($"https://www.imdb.com/title/{ item.ImdbId}")
                    .WithImageUrl(item.Poster)
                    .AddField("Rating", $"{item.ImdbRating}\nMetascore: {item.Metascore}")
                    .AddField("Info", $"Director: {item.Director}\nWriter: {item.Writer}\nCast: {item.Actors}\nGenre: {item.Genre}\nCountry: {item.Country}")
                    .AddField("Release dates", $"Released: {item.Released}\nDVD: {item.Dvd}", true)
                    .AddField("Trivia", $"Box office: {item.BoxOffice}\nAwards: {item.Awards}"));
            }

            return pages;
        }

        internal async Task<IEnumerable<DiscordEmbedBuilder>> UrbanDictionaryAsync(CommandContext ctx, string query)
        {
            query = query.Replace(' ', '+');
            var model = await Helpers.GetFromJsonAsync<UrbanDictionaryModel>($"http://api.urbandictionary.com/v0/define?term={query}");

            if (model?.List is null || model.List.Count == 0)
                throw new HttpRequestException("UrbanDictionary did not return any result", null, HttpStatusCode.BadRequest);

            var pages = new List<DiscordEmbedBuilder>();
            foreach (var item in model.List.Where(i => i is not null).OrderByDescending(x => Helpers.WilsonRating(x!.ThumbsUp ?? 0, x.ThumbsDown ?? 0)))
            {
                var desc = new StringBuilder(item!.Definition?.Replace("[", "").Replace("]", ""));
                if (!string.IsNullOrEmpty(item.Example))
                    desc.AppendLine()
                        .AppendLine(Formatter.Italic("Example:"))
                        .AppendLine(item.Example?.Replace("[", "").Replace("]", ""));

                pages.Add(new DiscordEmbedBuilder()
                    .WithAuthor("Urban Dictionary", "https://urbandictionary.com", "https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png")
                    .WithColor(new DiscordColor(255, 84, 33))
                    .WithTitle($"Search results for {Formatter.Italic(query)}:")
                    .AddField($"{item.Word ?? query} ({item.ThumbsUp} {DiscordEmoji.FromName(ctx.Client, ":thumbsup:")} / {item.ThumbsDown} {DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")})", desc.ToString()));
            }

            return pages;
        }

        internal Task<IEnumerable<DiscordEmbedBuilder>> WeatherAsync(string query)
        {
            throw new NotImplementedException();
        }

        internal async Task<IEnumerable<DiscordEmbedBuilder>> WikipediaAsync(string query)
        {
            var responseObject = await Helpers.GetFromJsonAsync<JsonArray>($"https://en.wikipedia.org/w/api.php?action=opensearch&search={query}");
            var titles = responseObject?[1]?.AsArray();
            var descriptions = responseObject?[2]?.AsArray();
            var urls = responseObject?[3]?.AsArray();

            var pages = new List<DiscordEmbedBuilder>();
            for (var i = 0; i < titles?.Count; i++)
            {
                var desc = urls?[i]?.ToString() + Environment.NewLine + descriptions?[i]?.ToString();
                pages.Add(new DiscordEmbedBuilder()
                    .WithAuthor("Wikipedia", "https://en.wikipedia.org/wiki/Main_Page", "https://upload.wikimedia.org/wikipedia/commons/d/de/Wikipedia_Logo_1.0.png")
                    .WithColor(DiscordColor.White)
                    .WithTitle($"Search results for {Formatter.Italic(query)}:")
                    .AddField(titles[i]?.ToString(), desc));
            }

            return pages;
        }

        internal async Task<string> YouTubeAsync(string query)
        {
            var request = _youTube.Search.List("snippet");
            request.Q = query;
            request.MaxResults = 1;
            request.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
            request.Type = "video";
            request.RelevanceLanguage = "en";
            var response = await request.ExecuteAsync();

            if (response.Items.Count == 0)
                throw new HttpRequestException($"{nameof(YouTubeService)} did not return any result", null, HttpStatusCode.BadRequest);

            return $"https://www.youtube.com/watch?v={response.Items[0].Id.VideoId}";
        }
    }
}
