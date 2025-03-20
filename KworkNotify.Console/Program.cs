using KworkNotify.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace KworkNotify.Console;

internal static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine("logs", $"{DateTime.Now:dd-MM-yyyy}.log"))
            .CreateLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder();
            
            Env.Load();
            var cookies = Environment.GetEnvironmentVariable("COOKIES");
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

            if (string.IsNullOrEmpty(cookies) || string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(botToken))
            {
                throw new ApplicationException("Missing environment variables: COOKIES or CONNECTION_STRING or BOT_TOKEN");
            }
            
            builder.Services.AddSingleton(_ => new KworkCookies()
            {
                Cookies = cookies
            });
            builder.Services.AddSingleton(_ => new TelegramToken()
            {
                Token = botToken
            });
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
            builder.Services.AddSerilog();
            builder.Services.AddSingleton<MongoContext>(_ => new MongoContext(connectionString));
            builder.Services.AddSingleton<KworkParser>();
            builder.Services.AddSingleton<KworkService>();
            builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<KworkService>());
            builder.Services.AddHostedService<TelegramService>();

            var app = builder.Build();
            await app.RunAsync();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Fatal error occured!");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}