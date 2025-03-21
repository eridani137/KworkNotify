namespace KworkNotify.Core.Kwork;

public class KworkProjectArgs(Project project) : EventArgs
{
    public Project Project { get; set; } = project;
}