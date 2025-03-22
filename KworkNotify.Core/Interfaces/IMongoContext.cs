using KworkNotify.Core.Kwork;
using KworkNotify.Core.Models;
using KworkNotify.Core.Telegram;
using MongoDB.Driver;

namespace KworkNotify.Core.Interfaces;

public interface IMongoContext
{
    IMongoCollection<TelegramUser> Users { get; }
    IMongoCollection<ApiUser> ApiUsers { get; }
    IMongoCollection<KworkProject> Projects { get; set; }
}