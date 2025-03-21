using KworkNotify.Core.Models;

namespace KworkNotify.Core.Auth;

public interface IUserService
{
    Task<ApiUser?> Authenticate(string username, string password);
}