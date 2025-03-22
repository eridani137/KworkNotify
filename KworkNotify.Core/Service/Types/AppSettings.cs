namespace KworkNotify.Core.Service.Types;

public class AppSettings
{
    public required string SiteUrl { get; init; }
    public required int PagesAmount { get; init; }
    public required int MinDelay { get; init; }
    public required int MaxDelay { get; init; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public required Dictionary<string, string> Headers { get; init; } = new();
    // ReSharper disable once CollectionNeverUpdated.Global
    public required List<long> AdminIds { get; init; } = [];
    public required string BackupScriptPath { get; init; }
    public required string BackupWorkingDirectory { get; init; }
    public required int BackupInterval { get; init; }
}