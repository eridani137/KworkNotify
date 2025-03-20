using KworkNotify.Core.Models;

namespace KworkNotify.Core;

public class KworkProjectArgs(Project project) : EventArgs
{
    public Project Project { get; set; } = project;
}