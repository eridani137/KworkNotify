using System.Text.RegularExpressions;
using KworkNotify.Core.Telegram;
using MongoDB.Driver;
using Serilog;

namespace KworkNotify.Core.Service;

public static partial class Extensions
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
    
    public static string EscapeMarkdownV2(this string input)
    {
        return EscapeSymbols().Replace(input, @"\$1");
    }
    
    public static string EnabledOrDisabledToEmoji(this bool value)
    {
        return value ? "\ud83d\udfe2" : "\ud83d\udd34";
    }

    [GeneratedRegex(@"([_*\[\]()~`>#+\-=|{}.!])")]
    private static partial Regex EscapeSymbols();
}