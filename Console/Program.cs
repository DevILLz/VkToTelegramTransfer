using VkToTelegramLib.Db;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Telegram;
using VkToTelegramLib.Vk;

TelegramBotService bot = null;

// Типа DI контейнер =)
try
{
    var config = BotConfiguration.GetOrCreateConfig();
    IVkService vkApi = new VkService(config);
    IDbContext dbContext = new DbContext();

    bot = new TelegramBotService(config, vkApi, dbContext);
    await Task.Run(() => bot.StartBot());
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
