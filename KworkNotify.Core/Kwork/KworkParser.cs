using Flurl.Http;
using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core.Kwork;

public class KworkParser(IOptions<AppSettings> settings) : IKworkParser
{
    private readonly Boundary _boundary = new();
    private readonly Random _random = new();

    public async IAsyncEnumerable<KworkProject> GetUpdate()
    {
        var pages = Enumerable.Range(1, settings.Value.PagesAmount).ToList();
        foreach (var page in pages)
        {
            if (page > 1)
            {
                var delay = _random.Next(3000, 7000);
                Log.ForContext<KworkParser>().Information("Delay {S} seconds", TimeSpan.FromMilliseconds(delay).TotalSeconds.ToString("F0"));
                await Task.Delay(delay);
            }
            
            Log.ForContext<KworkParser>().Information("Load page {Page} of {PagesCount}", page, pages.Count);
            
            var loadProjects = ParsePage(page);
            if (loadProjects == null) continue;
            await foreach (var kworkProject in loadProjects)
            {
                yield return kworkProject;
            }
        }
    }

    public async IAsyncEnumerable<KworkProject>? ParsePage(int page)
    {
        var boundaryData = _boundary.GetBoundaryData(page);
        
        var referer = $"{settings.Value.SiteUrl}/projects";

        var receive = await $"{settings.Value.SiteUrl}/projects"
            .WithHeaders(GetHeaders(referer))
            .PostAsync(new StringContent(boundaryData))
            .ReceiveJson<KworkResponse>();

        foreach (var project in receive.Data.Data.Projects)
        {
            yield return project;
        }
    }

    public Dictionary<string, string> GetHeaders(string referer)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in settings.Value.Headers)
        {
            headers.TryAdd(header.Key, header.Value);
        }
        headers.Add("Content-Type", $"multipart/form-data; boundary={_boundary.BoundaryBody}");
        headers.Add("Referer", referer);
        headers.Add("Origin", settings.Value.SiteUrl);
        return headers;
    }
}