using System.Diagnostics.CodeAnalysis;

namespace LennyBOTv3.Models
{
    public record JobModel
    {
        public bool Enabled { get; init; }

        public TimeSpan Interval { get; init; }

        public DateTime LastRunUtc { get; init; }

        [LiteDB.BsonId, NotNull]
        public string? Name { get; init; }

        public bool RepeatOnError { get; init; }
        public bool Running { get; init; }
    }
}
