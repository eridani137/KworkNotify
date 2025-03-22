using KworkNotify.Core.Kwork;

namespace KworkNotify.Core.Interfaces;

public interface IKworkParser
{
    IAsyncEnumerable<KworkProject> GetUpdate();
    IAsyncEnumerable<KworkProject>? ParsePage(int page);
    Dictionary<string, string> GetHeaders(string referer);
}