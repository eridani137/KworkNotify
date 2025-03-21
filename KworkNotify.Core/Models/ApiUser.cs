using MongoDB.Bson;

namespace KworkNotify.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class ApiUser
{
    public ObjectId Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}