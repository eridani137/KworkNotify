using KworkNotify.Core.Models;
using KworkNotify.Core.Service;
using MongoDB.Driver;

namespace KworkNotify.Core.Auth;

public class UserService(MongoContext context) : IUserService
{
    public async Task<ApiUser?> Authenticate(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;
        var user = await context.ApiUsers.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null) return null;
        return !password.Equals(user.Password, StringComparison.Ordinal) ? null : user;
    }
}