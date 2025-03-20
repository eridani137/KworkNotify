using Flurl.Http;
using KworkNotify.Core.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core;

public class KworkParser(IOptions<AppSettings> settings, KworkCookies kworkCookies)
{
    private readonly BoundaryHelper _boundary = new();
    private readonly Random _random = new();

    public async IAsyncEnumerable<Project> GetUpdate()
    {
        var pages = Enumerable.Range(1, settings.Value.PagesAmount).ToList();
        foreach (var page in pages)
        {
            if (page > 1)
            {
                var delay = _random.Next(3000, 7000);
                Log.Information("Delay {S} seconds", TimeSpan.FromMilliseconds(delay).TotalSeconds.ToString("F0"));
                await Task.Delay(delay);
            }
            
            Log.Information("Load page {Page} of {PagesCount}", page, pages.Count);
            
            var loadProjects = ParsePage(page);
            if (loadProjects == null) continue;
            await foreach (var kworkProject in loadProjects)
            {
                yield return kworkProject;
            }
        }
    }
    
    private async IAsyncEnumerable<Project>? ParsePage(int page)
    {
        var boundaryData = _boundary.GetBoundaryData(page);
        
        var referer = $"{settings.Value.SiteUrl}/projects";

        var receive = await $"{settings.Value.SiteUrl}/projects"
            .WithCookies(kworkCookies.Cookies)
            .WithHeaders(GetHeaders(referer))
            .PostAsync(new StringContent(boundaryData))
            .ReceiveJson<KworkResponse>();

        foreach (var project in receive.Data.Data.Projects)
        {
            yield return project;
        }
    }

    private Dictionary<string, string> GetHeaders(string referer)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in settings.Value.Headers)
        {
            headers.TryAdd(header.Key, header.Value);
        }
        headers.Add("Content-Type", $"multipart/form-data; boundary={_boundary.Boundary}");
        headers.Add("Referer", referer);
        headers.Add("Origin", settings.Value.SiteUrl);
        return headers;
    }
}