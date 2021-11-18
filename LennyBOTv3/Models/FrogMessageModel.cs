namespace LennyBOTv3.Models
{
    public record FrogMessageModel
    {
        [LiteDB.BsonId]
        public ulong UserId { get; init; }

        public DateTime TimeUtc { get; init; }

        public DateTime LastSendUtc { get; init; }
    }
}
