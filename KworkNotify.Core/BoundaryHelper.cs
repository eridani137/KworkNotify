using System.Text;

namespace KworkNotify.Core;

public class BoundaryHelper
{
    public readonly string Boundary = $"------WebKitFormBoundary{DateTime.Now.Ticks:x}";
    
    public string GetBoundaryData(int pageNumber)
    {
        var builder = new StringBuilder();
            
        builder.AppendLine($"{Boundary}")
            .AppendLine("Content-Disposition: form-data; name=\"a\"")
            .AppendLine()
            .AppendLine("1")
            .AppendLine($"{Boundary}")
            .AppendLine("Content-Disposition: form-data; name=\"page\"")
            .AppendLine()
            .AppendLine($"{pageNumber}")
            .Append($"{Boundary}--");

        return builder.ToString();
    }
}