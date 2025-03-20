using MongoDB.Driver;
using Serilog;

namespace KworkNotify.Core;

public static class Extensions
{
    public static async Task<TelegramUser?> GetOrAddUser(this MongoContext context, long userId, TelegramRole role = TelegramRole.User)
    {
        try
        {
            var user = await context.Users.Find(u => u.Id == userId)
                .FirstOrDefaultAsync();
            if (user != null) return user;
            user = new TelegramUser()
            {
                Id = userId,
                Role = role,
                SendUpdates = true
            };
            await context.Users.InsertOneAsync(user);
            return user;
        }
        catch (Exception e)
        {
            Log.Error(e, "GetOrAddUser");
            return null;
        }
    }
    
    public static string EnabledOrDisabledToEmoji(this bool value)
    {
        return value ? "\ud83d\udfe2" : "\ud83d\udd34";
    }
}