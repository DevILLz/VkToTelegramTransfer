using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Telegram;

namespace WorkerService;

public class TelegramBotWorker(TelegramBotService bot, ILogger<TelegramBotWorker> logger) : BackgroundService
{
    private readonly ILogger<TelegramBotWorker> logger = logger;
    private TelegramBotService bot = bot;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

        try
        {
            bot.StartBot(cancellationToken);
        }
        catch (Exception)
        {
            logger.LogCritical($"Возникли проблемы при запуске бота... \n" +
                $"Проверьте конфигурационный файл: \n {BotConfiguration.BaseDirectory}\\{BotConfiguration.ConfigFileName}\n" +
                $"");
        }

        if (bot?.Initialized ?? false)
            logger.LogInformation("Бот запущен успешно");
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Service is stopping...");
        await base.StopAsync(cancellationToken);
        bot?.Dispose();
    }
}
