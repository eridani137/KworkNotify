namespace KworkNotify.Core.Service;

public class AppSettings
{
    public required string SiteUrl { get; init; }
    public required int PagesAmount { get; init; }
    public required int MinDelay { get; init; }
    public required int MaxDelay { get; init; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public required Dictionary<string, string> Headers { get; init; } = new();
    public required List<long> AdminIds { get; init; } = [];
}