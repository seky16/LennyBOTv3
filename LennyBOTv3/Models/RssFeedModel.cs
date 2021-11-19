using System.Diagnostics.CodeAnalysis;
using LiteDB;

namespace LennyBOTv3.Models
{
    public record RssFeedModel
    {
        public bool Enabled { get; init; }

        [BsonId]
        public uint Hash => (ChannelId + Url).Checksum();

        public ulong ChannelId { get; init; }
        public string? LastItemId { get; init; }
        public DateTime? LastUpdatedUtc { get; init; }

        [NotNull]
        public string? Name { get; init; }

        [NotNull]
        public string? Url { get; init; }
    }
}
