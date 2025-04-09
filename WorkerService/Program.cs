using Serilog;
using VkToTelegramLib.Db;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Telegram;
using VkToTelegramLib.Vk;
using WorkerService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(BotConfiguration.BaseDirectory + "\\log.txt", rollingInterval: RollingInterval.Day) // лог по дням
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // <--- ключевая строка
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
        logging.AddEventLog(s => s.Filter = (f, b) => b >= LogLevel.Warning);    // Добавим вывод в журнал Windows (если надо)
    })
    .Build();

host.Run();

//https://github.com/TelegramBots/Telegram.Bot
//https://dev.vk.com/ru/method/wall