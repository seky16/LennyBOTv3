using System.Text.Json.Serialization;

namespace LennyBOTv3.Models
{
    public record List
    {
        [JsonPropertyName("author")]
        public string? Author { get; init; }

        [JsonPropertyName("current_vote")]
        public string? CurrentVote { get; init; }

        [JsonPropertyName("defid")]
        public long? Defid { get; init; }

        [JsonPropertyName("definition")]
        public string? Definition { get; init; }

        [JsonPropertyName("example")]
        public string? Example { get; init; }

        [JsonPropertyName("permalink")]
        public Uri? Permalink { get; init; }

        [JsonPropertyName("sound_urls")]
        public List<Uri?>? SoundUrls { get; init; }

        [JsonPropertyName("thumbs_down")]
        public long? ThumbsDown { get; init; }

        [JsonPropertyName("thumbs_up")]
        public long? ThumbsUp { get; init; }

        [JsonPropertyName("word")]
        public string? Word { get; init; }

        [JsonPropertyName("written_on")]
        public DateTimeOffset? WrittenOn { get; init; }
    }

    public record UrbanDictionaryModel
    {
        [JsonPropertyName("list")]
        public List<List?>? List { get; init; }
    }
}
