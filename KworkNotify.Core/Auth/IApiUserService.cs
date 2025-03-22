using KworkNotify.Core.Models;

namespace KworkNotify.Core.Auth;

public interface IApiUserService
{
    Task<ApiUser?> Authenticate(string username, string password);
}