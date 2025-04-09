using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;
using VkToTelegramLib.Db;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Telegram;
using VkToTelegramLib.Vk;
using WorkerService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(BotConfiguration.BaseDirectory + "\\log.txt", rollingInterval: RollingInterval.Day) // лог по дням
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        services.AddSingleton<TelegramBotService>();
        services.AddSingleton(typeof(IDbContext), typeof(DbContext));
        services.AddSingleton(typeof(IVkService), typeof(VkService));
        services.AddSingleton(typeof(BotConfiguration), BotConfiguration.GetOrCreateConfig());
        services.AddHostedService<TelegramBotWorker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
#pragma warning disable CA1416 // Validate platform compatibility
        logging.AddEventLog(s => s.Filter = (f, b) => b >= LogLevel.Warning);    // Добавим вывод в журнал Windows (если надо)
#pragma warning restore CA1416 // Validate platform compatibility
    });

bool isService = !(Debugger.IsAttached || args.Contains("--console"));
if (isService && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    host = host.UseWindowsService();
}

host.Build().Run();

//https://github.com/TelegramBots/Telegram.Bot
//https://dev.vk.com/ru/method/wall