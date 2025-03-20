using System.Text.Json.Serialization;
using KworkNotify.Core.Models;

namespace KworkNotify.Core;

// ReSharper disable once ClassNeverInstantiated.Global
public class KworkResponse
{
    [JsonPropertyName("success")] public required bool IsSuccess { get; set; }
    [JsonPropertyName("data")] public required KworkResponseData Data { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class KworkResponseData
{
    [JsonPropertyName("pagination")] public required KworkResponseDataPaginationData Data { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class KworkResponseDataPaginationData
{
    [JsonPropertyName("current_page")] public required int CurrentPage { get; set; }
    // ReSharper disable once CollectionNeverUpdated.Global
    [JsonPropertyName("data")] public required List<Project> Projects { get; set; } = [];
}