using KworkNotify.Core.Interfaces;

namespace KworkNotify.Core.Service.Types;

public class AppSettings : IAppSettings
{
    public required bool IsDebug { get; set; }
    public required string SiteUrl { get; init; }
    public required int PagesAmount { get; init; }
    public required int MinDelay { get; init; }
    public required int MaxDelay { get; init; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public required Dictionary<string, string> Headers { get; init; } = new();
    public required string BackupScriptPath { get; init; }
    public required int BackupInterval { get; init; }
    public required int StatisticPushInterval { get; init; }
}