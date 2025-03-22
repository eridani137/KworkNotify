using System.Text;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KworkNotify.Core.Kwork;

// ReSharper disable once ClassNeverInstantiated.Global
public class KworkProject
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    [JsonPropertyName("id")] public required int Id { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("wantUserGetProfileUrl")] public required string UserUrl { get; set; }
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("category_id")] public required string CategoryId { get; set; }
    [JsonPropertyName("possiblePriceLimit")] public required int PossiblePriceLimit { get; set; }
    [JsonPropertyName("priceLimit")] public required string PriceLimit { get; set; }
    [JsonPropertyName("getWantsActiveCount")] public required string GetWantsActiveCount { get; set; }
    [JsonPropertyName("max_days")] public required string MaxDays { get; set; }
    [JsonPropertyName("user")] public required KworkUser KworkUser { get; set; }

    public string GetKey => $"project:{Id}";
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪");
        builder.AppendLine(Name);
        builder.AppendLine("🌐 |SET_URL_HERE|");
        builder.AppendLine();
        builder.AppendLine($"💵 Цена: {PriceLimit.Replace(".00", "")} - {PossiblePriceLimit}");
        builder.AppendLine($"⏳ Закончить за: {MaxDays} дней");
        builder.AppendLine($"🤼‍♂️ Предложений: {GetWantsActiveCount}");
        builder.AppendLine($"🙋‍♂️ Покупатель: {KworkUser.UserName}");
        builder.AppendLine($"🎫 Нанято: {KworkUser.UserData.WantsHiredPercent}%");
        builder.AppendLine($"⚒️ Размещено проектов: {KworkUser.UserData.WantsCount}");
        builder.AppendLine();
        builder.AppendLine($"📋 {Description}");
        builder.AppendLine("🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪🟪");
            
        return builder.ToString();
    }
}