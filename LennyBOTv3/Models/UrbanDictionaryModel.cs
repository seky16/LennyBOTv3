﻿using System.Text.Json.Serialization;

namespace LennyBOTv3.Models
{
    public record UrbanDictionaryModel
    {
        [JsonPropertyName("list")]
        public List<List?>? List { get; init; }
    }

    public record List
    {
        [JsonPropertyName("definition")]
        public string? Definition { get; init; }

        [JsonPropertyName("permalink")]
        public Uri? Permalink { get; init; }

        [JsonPropertyName("thumbs_up")]
        public long? ThumbsUp { get; init; }

        [JsonPropertyName("sound_urls")]
        public List<Uri?>? SoundUrls { get; init; }

        [JsonPropertyName("author")]
        public string? Author { get; init; }

        [JsonPropertyName("word")]
        public string? Word { get; init; }

        [JsonPropertyName("defid")]
        public long? Defid { get; init; }

        [JsonPropertyName("current_vote")]
        public string? CurrentVote { get; init; }

        [JsonPropertyName("written_on")]
        public DateTimeOffset? WrittenOn { get; init; }

        [JsonPropertyName("example")]
        public string? Example { get; init; }

        [JsonPropertyName("thumbs_down")]
        public long? ThumbsDown { get; init; }
    }
}
