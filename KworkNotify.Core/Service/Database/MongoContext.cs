using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Kwork;
using KworkNotify.Core.Models;
using MongoDB.Driver;

namespace KworkNotify.Core.Service.Database;

public class MongoContext : IMongoContext
{
    public IMongoCollection<ApiUser> ApiUsers { get; }
    public IMongoCollection<KworkProject> Projects { get; set; }
    
    public MongoContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("kwork_notify");
        ApiUsers = database.GetCollection<ApiUser>("api_users");
        Projects = database.GetCollection<KworkProject>("projects");
    }
}