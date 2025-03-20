using MongoDB.Driver;

namespace KworkNotify.Core;

public class MongoContext
{
    public MongoContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("kwork_notify");
    }
}