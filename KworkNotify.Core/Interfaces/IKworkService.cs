using KworkNotify.Core.Kwork;

namespace KworkNotify.Core.Interfaces;

public interface IKworkService
{
    event Func<object?, KworkProjectArgs, Task>? AddedNewProject;
    Task Worker();
    Task OnAddedNewProject(KworkProjectArgs args);
}