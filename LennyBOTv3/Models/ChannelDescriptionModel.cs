﻿namespace LennyBOTv3.Models
{
    public record ChannelDescriptionModel
    {
        public DateTime DateTimeUtc { get; init; }

        [LiteDB.BsonId]
        public ulong ChannelId { get; init; }

        public string? Text { get; init; }

        public string GetTopic(DateTime utcNow)
        {
            var eta = (utcNow - DateTimeUtc).TotalDays;
            return Text?.Replace("{eta}", (eta > 0 ? "+" : "") + eta.ToString("N0")) ?? string.Empty;
        }
    }
}
