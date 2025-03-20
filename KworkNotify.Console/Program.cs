using KworkNotify.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;
using DotNetEnv;

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
            var cookies = Environment.GetEnvironmentVariable("COOKIES")!;
            builder.Services.AddSingleton(_ => new KworkCookies()
            {
                Cookies = cookies
            });
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
            builder.Services.AddSerilog();
            builder.Services.AddSingleton<KworkParser>();
            builder.Services.AddHostedService<KworkService>();

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