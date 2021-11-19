namespace LennyBOTv3.Models
{
    public record FrogMessageModel
    {
        public DateTime LastSendUtc { get; init; }

        public DateTime TimeUtc { get; init; }

        [LiteDB.BsonId]
        public ulong UserId { get; init; }
    }
}
