using KworkNotify.Core.Kwork;
using KworkNotify.Core.Models;
using KworkNotify.Core.Telegram;
using MongoDB.Driver;

namespace KworkNotify.Core.Service;

public class MongoContext
{
    public IMongoCollection<TelegramUser> Users { get; }
    public IMongoCollection<ApiUser> ApiUsers { get; }
    public IMongoCollection<KworkProject> Projects { get; set; }
    
    public MongoContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("kwork_notify");
        Users = database.GetCollection<TelegramUser>("users");
        ApiUsers = database.GetCollection<ApiUser>("api_users");
        Projects = database.GetCollection<KworkProject>("projects");
    }
}