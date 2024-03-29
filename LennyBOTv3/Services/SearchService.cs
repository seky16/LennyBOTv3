﻿using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Alpaca.Markets;
using Alpaca.Markets.Extensions;
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
    public class SearchService : LennyBaseService<SearchService>
    {
        private readonly IAlpacaDataClient _alpacaData;
        private readonly IAlpacaTradingClient _alpacaTrading;
        private readonly ApiSettings _apiSettings;
        private readonly AsyncOmdbClient _omdb;
        private readonly YouTubeService _youTube;

        public SearchService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
            _omdb = serviceProvider.GetRequiredService<AsyncOmdbClient>();
            _youTube = serviceProvider.GetRequiredService<YouTubeService>();
            _alpacaData = serviceProvider.GetRequiredService<IAlpacaDataClient>();
            _alpacaTrading = serviceProvider.GetRequiredService<IAlpacaTradingClient>();
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> ImdbAsync(string query)
        {
            var list = await _omdb.GetSearchListAsync(query);
            if (!string.IsNullOrEmpty(list.Error))
                throw new HttpRequestException($"OmdbApi returned an error: {list.Error}", null, HttpStatusCode.BadRequest);

            if (list.SearchResults.Count == 0)
                throw new HttpRequestException("OmdbApi did not return any result", null, HttpStatusCode.BadRequest);

            var pages = new List<DiscordEmbedBuilder>();
            foreach (var result in list.SearchResults)
            {
                var item = await _omdb.GetItemByIdAsync(result.ImdbId);
                pages.Add(new DiscordEmbedBuilder()
                    .WithAuthor("IMDb", "https://www.imdb.com/", "http://files.softicons.com/download/social-media-icons/flat-gradient-social-icons-by-guilherme-lima/png/512x512/IMDb.png")
                    .WithColor(DiscordColor.Goldenrod)
                    .WithTitle($"{item.Title} ({item.Year})")
                    .WithDescription($"{item.Plot} ({item.Runtime})")
                    .WithUrl($"https://www.imdb.com/title/{item.ImdbId}")
                    .WithImageUrl(item.Poster)
                    .AddField("Rating", $"{item.ImdbRating}\nMetascore: {item.Metascore}")
                    .AddField("Info", $"Director: {item.Director}\nWriter: {item.Writer}\nCast: {item.Actors}\nGenre: {item.Genre}\nCountry: {item.Country}")
                    .AddField("Release dates", $"Released: {item.Released}\nDVD: {item.Dvd}", true)
                    .AddField("Trivia", $"Box office: {item.BoxOffice}\nAwards: {item.Awards}"));
            }

            return pages;
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> StocksAsync(string symbol)
        {
            var pages = new List<DiscordEmbedBuilder>();
            var results = await _alpacaData.GetHistoricalBarsAsAsyncEnumerable(new HistoricalBarsRequest(symbol,
                DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddMinutes(-15), BarTimeFrame.Day)).ToListAsync();

            foreach (var bar in results)
            {
                pages.Add(new DiscordEmbedBuilder()
                    .WithAuthor("Alpaca", "https://alpaca.markets/", "https://files.alpaca.markets/webassets/apple-touch-icon.png")
                    .WithColor(new DiscordColor("#FFD700"))
                    .WithTitle($"Information for symbol: {Formatter.Bold(bar?.Symbol)}")
                    .WithDescription(Formatter.Timestamp(bar!.TimeUtc, TimestampFormat.LongDate))
                    .AddField(nameof(bar.Open), bar.Open.ToString(), true)
                    .AddField(nameof(bar.Close), bar.Close.ToString(), true)
                    .AddField("\u200B", "\u200B", true)
                    .AddField(nameof(bar.High), bar.High.ToString(), true)
                    .AddField(nameof(bar.Low), bar.Low.ToString(), true)
                    .AddField("\u200B", "\u200B", true)
                    );
            }

            if (pages.Count == 0)
                throw new HttpRequestException("Alpaca API did not return any result");

            var clock = await _alpacaTrading.GetClockAsync();
            if (clock is not null)
                pages.Last().AddField($"Market is {Formatter.Bold(clock.IsOpen ? "open" : "closed")}",
                    clock.IsOpen
                        ? $"Closes {Formatter.Timestamp(clock.NextCloseUtc)}"
                        : $"Opens {Formatter.Timestamp(clock.NextOpenUtc)}");

            pages.Reverse();
            return pages;
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> UrbanDictionaryAsync(CommandContext ctx, string query)
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
                    .AddField($"{item.Word ?? query} ({item.ThumbsUp} {DiscordEmoji.FromName(ctx.Client, ":thumbsup:")} / {item.ThumbsDown} {DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")})", desc.ToString().Truncate(1024)));
            }

            return pages;
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> WeatherAsync(DiscordUser user)
        {
            var location = await Database.GetUserLocationAsync(user);
            if (string.IsNullOrEmpty(location))
                throw new InvalidOperationException($"User doesn't have default location set. They can use 'settings location' command to set one up.");

            return await WeatherAsync(location);
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> WeatherAsync(string query)
        {
            var result = new List<DiscordEmbedBuilder?>()
            {
                await OpenWeatherMapAsync(query),
                await WeatherstackAsync(query),
            }.OfType<DiscordEmbedBuilder>();

            if (!result.Any())
                throw new HttpRequestException("Weather APIs returned no results", null, HttpStatusCode.BadRequest);

            return result;
        }

        public async Task<IEnumerable<DiscordEmbedBuilder>> WikipediaAsync(string query)
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
                    .AddField(titles[i]?.ToString(), desc?.Truncate(1024)));
            }

            return pages;
        }

        public async Task<string> YouTubeAsync(string query)
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

        private async Task<DiscordEmbedBuilder?> OpenWeatherMapAsync(string location)
        {
            var apiKey = _apiSettings.OpenWeatherMapApiKey;
            OpenWeatherMapModel? model;

            try
            {
                model = await Helpers.GetFromXmlAsync<OpenWeatherMapModel>($"https://api.openweathermap.org/data/2.5/weather?appid={apiKey}&q={location}&units=metric&mode=xml");
            }
            catch
            {
                return null;
            }

            if (model is null)
                return null;

            var updatedOnUtc = model.Lastupdate?.Value.Equals(default(DateTime)) ?? true ? DateTime.UtcNow : DateTime.SpecifyKind(model.Lastupdate.Value, DateTimeKind.Utc);

            return new DiscordEmbedBuilder()
                .WithTitle($"Weather in {model.City?.Name ?? location}, {model.City?.Country}")
                .WithDescription($"{model.Temperature?.Value} °C (min: {model.Temperature?.Min} °C; max: {model.Temperature?.Max} °C) {model.Weather?.Value}")
                .WithThumbnail($"https://openweathermap.org/img/wn/{model.Weather?.Icon}@2x.png")
                //.WithFooter($"Last update: {Formatter.Timestamp(updatedOnUtc, TimestampFormat.ShortDateTime)}")
                .WithAuthor("OpenWeather", "https://openweathermap.org/", "https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/icons/logo_60x60.png")
                .AddField("More info",
                ($"Feels like: {model.FeelsLike?.Value} °C\n" +
                $"Cloud coverage: {model.Clouds?.Value} % ({model.Clouds?.Name})\n" +
                $"Precipitation: {model.Precipitation?.Value} mm\n" +
                $"Humidity: {model.Humidity?.Value} {model.Humidity?.Unit}\n" +
                $"Pressure: {model.Pressure?.Value} {model.Pressure?.Unit}\n" +
                $"Wind: {model.Wind?.Speed?.Value} {model.Wind?.Speed?.Unit} {model.Wind?.Direction?.Code} ({model.Wind?.Speed?.Name})\n" +
                $"Visibility: {model.Visibility?.Value / 1000.0} km\n" +
                $"Sunrise/sunset: {model.City?.Sun?.Rise.AddSeconds(model.City?.Timezone ?? 0).TimeOfDay} / {model.City?.Sun?.Set.AddSeconds(model.City?.Timezone ?? 0).TimeOfDay} (local time)\n").Truncate(1024));
        }

        private async Task<DiscordEmbedBuilder?> WeatherstackAsync(string location)
        {
            var apiKey = _apiSettings.WeatherstackApiKey;
            WeatherstackModel? model;

            try
            {
                model = await Helpers.GetFromJsonAsync<WeatherstackModel>($"http://api.weatherstack.com/current?access_key={apiKey}&query={location}").ConfigureAwait(false);
            }
            catch
            {
                return null;
            }

            if (model?.Current is null || model.Location is null || !model.Success)
            {
                Logger.LogWarning("Weatherstack returned an error: {error}", model?.Error);
                return null;
            }

            var updatedOnUtc = DateTimeOffset.UtcNow;
            if (model.Location.LocaltimeEpoch.HasValue && model.Location.UtcOffset.HasValue)
            {
                updatedOnUtc = DateTimeOffset.FromUnixTimeSeconds(model.Location.LocaltimeEpoch.Value).AddHours(-model.Location.UtcOffset.Value);
            }

            return new DiscordEmbedBuilder()
                .WithTitle($"Weather in {model.Location.Name}, {model.Location.Region}, {model.Location.Country}")
                .WithDescription($"{model.Current.Temperature} °C {model.Current.WeatherDescriptions?.FirstOrDefault()}")
                .WithThumbnail(model.Current.WeatherIcons?.FirstOrDefault())
                //.WithFooter($"Last update: {Formatter.Timestamp(updatedOnUtc, TimestampFormat.ShortDateTime)}")
                .WithAuthor("weatherstack", "https://weatherstack.com", "https://weatherstack.com/site_images/weatherstack_icon.png")
                .AddField("More info",
                ($"Feels like: {model.Current.Feelslike} °C\n" +
                $"Cloud coverage: {model.Current.Cloudcover} %\n" +
                $"Precipitation: {model.Current.Precip} mm\n" +
                $"Humidity: {model.Current.Humidity} %\n" +
                $"Pressure: {model.Current.Pressure} mBar\n" +
                $"Wind: {model.Current.WindSpeed} km/h {model.Current.WindDir}\n" +
                $"Visibility: {model.Current.Visibility} km\n" +
                $"UV Index: {model.Current.UVIndex}").Truncate(1024));
        }
    }
}
