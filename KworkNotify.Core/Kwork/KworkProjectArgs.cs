namespace KworkNotify.Core.Kwork;

public class KworkProjectArgs(KworkProject kworkProject) : EventArgs
{
    public KworkProject KworkProject { get; set; } = kworkProject;
}