using System.Text.RegularExpressions;
using KworkNotify.Core.Interfaces;
using MongoDB.Driver;
using Serilog;

namespace KworkNotify.Core.Telegram;

public static partial class TelegramExtensions
{
    public static async Task<TelegramUser?> OnOpenedForm(this IMongoContext context, IAppCache redis, IAppSettings settings, long chatId)
    {
        var role = TelegramRole.User;
        if (settings.AdminIds.Contains(chatId))
        {
            role = TelegramRole.Admin;
        }
        return await context.GetOrAddUser(redis, chatId, role);
    }
    
    public static async Task<TelegramUser?> GetOrAddUser(this IMongoContext context, IAppCache redis, long userId, TelegramRole role = TelegramRole.User)
    {
        var cacheKey = userId.ToKey();
        var expiry = TimeSpan.FromHours(10);
        
        try
        {
            var cachedUser = await redis.GetAsync<TelegramUser>(cacheKey);
            if (cachedUser != null)
            {
                return cachedUser;
            }
            
            var user = await context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();
            if (user != null)
            {
                await redis.SetAsync(cacheKey, user, expiry);
                return user;
            }
            
            user = new TelegramUser
            {
                Id = userId,
                Role = role,
                SendUpdates = true
            };
            
            await context.Users.InsertOneAsync(user);
            await redis.SetAsync(cacheKey, user, expiry);
            return user;
        }
        catch (Exception e)
        {
            Log.Error(e, "Unexpected error in GetOrAddUser for userId: {UserId}", userId);
            return null;
        }
    }
    
    public static string ToKey(this long userId)
    {
        return $"user:{userId.ToString()}";
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