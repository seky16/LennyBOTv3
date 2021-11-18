using System.Diagnostics.CodeAnalysis;

namespace LennyBOTv3.Models
{
    public record JobModel
    {
        [LiteDB.BsonId, NotNull]
        public string? Name { get; init; }
        public DateTime LastRunUtc { get; init; }
        public TimeSpan Interval { get; init; }
        public bool Enabled { get; init; }
        public bool RepeatOnError { get; init; }
        public bool Running { get; init; }
    }
}
