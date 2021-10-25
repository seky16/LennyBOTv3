namespace LennyBOTv3.Models
{
    public record RssFeedModel
    {
        public ulong ChannelId { get; init; }
        public DateTime LastUpdatedUtc { get; init; }
        public string Name { get; init; }
        public string Url { get; init; }
    }
}
