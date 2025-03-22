using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Models;
using MongoDB.Driver;

namespace KworkNotify.Core.Auth;

public class ApiUserService(IMongoContext context) : IApiUserService
{
    public async Task<ApiUser?> Authenticate(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;
        var user = await context.ApiUsers.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null) return null;
        return !password.Equals(user.Password, StringComparison.Ordinal) ? null : user;
    }
}