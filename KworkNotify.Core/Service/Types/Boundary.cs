using System.Text;

namespace KworkNotify.Core.Service.Types;

public class Boundary
{
    public readonly string BoundaryBody = $"------WebKitFormBoundary{DateTime.Now.Ticks:x}";
    
    public string GetBoundaryData(int pageNumber)
    {
        var builder = new StringBuilder();
            
        builder.AppendLine($"{BoundaryBody}")
            .AppendLine("Content-Disposition: form-data; name=\"a\"")
            .AppendLine()
            .AppendLine("1")
            .AppendLine($"{BoundaryBody}")
            .AppendLine("Content-Disposition: form-data; name=\"page\"")
            .AppendLine()
            .AppendLine($"{pageNumber}")
            .Append($"{BoundaryBody}--");

        return builder.ToString();
    }
}