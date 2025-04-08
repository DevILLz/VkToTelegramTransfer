using HotBike.Telegram.Bot;
using HotBike.Telegram.Bot.DB;
using HotBike.Telegram.Bot.Services;

TelegramBot bot = null;

// Типа DI контейнер =)
try
{
    var config = BotConfiguration.GetOrCreateConfig();
    var vkApi = new VkApi(config);
    var dbContext = new DbContext();

    bot = new TelegramBot(config, vkApi, dbContext);
    await Task.Run(bot.StartBot);
}
catch (Exception)
{
    Console.WriteLine($"Возникли проблемы при запуске бота... \n" +
        $"Проверьте конфигурационный файл: \n {BotConfiguration.BaseDirectory}\\{BotConfiguration.ConfigFileName}\n" +
        $"");
}

if (bot?.Initialized ?? false)
    Console.WriteLine("Бот запущен успешно");

Console.ReadLine();


//https://github.com/TelegramBots/Telegram.Bot
//https://dev.vk.com/ru/method/wall
