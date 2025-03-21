using KworkNotify.Core.Models;
using MongoDB.Driver;

namespace KworkNotify.Core;

public class MongoContext
{
    public IMongoCollection<TelegramUser> Users { get; }
    public IMongoCollection<ApiUser> ApiUsers { get; }
    public IMongoCollection<Project> Projects { get; set; }
    
    public MongoContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("kwork_notify");
        Users = database.GetCollection<TelegramUser>("users");
        ApiUsers = database.GetCollection<ApiUser>("api_users");
        Projects = database.GetCollection<Project>("projects");
    }
}