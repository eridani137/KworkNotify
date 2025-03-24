using KworkNotify.Core.Kwork;
using KworkNotify.Core.Models;
using MongoDB.Driver;

namespace KworkNotify.Core.Interfaces;

public interface IMongoContext
{
    IMongoCollection<ApiUser> ApiUsers { get; }
    IMongoCollection<KworkProject> Projects { get; set; }
}