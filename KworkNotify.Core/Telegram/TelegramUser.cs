using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KworkNotify.Core.Telegram;

public class TelegramUser
{
    [BsonId]
    [BsonRepresentation(BsonType.Int64)]
    public long Id { get; init; }
    public TelegramRole Role { get; set; }
    public bool SendUpdates { get; set; }
    public int ReceivedMessages { get; set; }
}