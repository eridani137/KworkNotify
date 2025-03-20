using System.Text;
using System.Text.Json.Serialization;

namespace KworkNotify.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class Project
{
    [JsonPropertyName("id")] public required int ProjectId { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("wantUserGetProfileUrl")] public required string UserUrl { get; set; }
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("category_id")] public required string CategoryId { get; set; }
    [JsonPropertyName("possiblePriceLimit")] public required int PossiblePriceLimit { get; set; }
    [JsonPropertyName("priceLimit")] public required string PriceLimit { get; set; }
    [JsonPropertyName("getWantsActiveCount")] public required string GetWantsActiveCount { get; set; }
    [JsonPropertyName("max_days")] public required string MaxDays { get; set; }
    [JsonPropertyName("user")] public required User User { get; set; }

    public override string ToString()
    {
        var builder = new StringBuilder();
    
        builder.AppendLine(Name);
        builder.AppendLine();
        builder.AppendLine($"💵 Цена: {PriceLimit} - {PossiblePriceLimit}");
        builder.AppendLine($"⏳ Закончить за: {MaxDays} дней");
        builder.AppendLine($"🤼‍♂️ Предложений: {GetWantsActiveCount}");
        builder.AppendLine($"🙋‍♂️ Покупатель: {User.UserName}");
        builder.AppendLine($"🎫 Нанято: {User.UserData.WantsHiredPercent}%");
        builder.AppendLine($"⚒️ Размещено проектов: {User.UserData.WantsCount}");
        builder.AppendLine();
        builder.AppendLine($"📋 {Description}");
        builder.AppendLine();
            
        return builder.ToString();
    }
}