using WorkerService;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // <--- ключевая строка
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

//https://github.com/TelegramBots/Telegram.Bot
//https://dev.vk.com/ru/method/wall