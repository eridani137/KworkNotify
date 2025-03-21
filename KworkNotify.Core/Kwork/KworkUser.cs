using System.Text.Json.Serialization;

namespace KworkNotify.Core.Kwork;

// ReSharper disable once ClassNeverInstantiated.Global
public class KworkUser
{
    [JsonPropertyName("USERID")] public required int UserId { get; set; }
    [JsonPropertyName("username")] public required string UserName { get; set; }
    [JsonPropertyName("data")] public required UserData UserData { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class UserData
{
    [JsonPropertyName("wants_count")] public required string WantsCount { get; set; }
    [JsonPropertyName("wants_hired_percent")] public required string WantsHiredPercent { get; set; }
}