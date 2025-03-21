using KworkNotify.Core.Models;

namespace KworkNotify.Api.Auth;

public interface IUserService
{
    Task<ApiUser?> Authenticate(string username, string password);
}