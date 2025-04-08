using VkToTelegramLib.Db;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Telegram;
using VkToTelegramLib.Vk;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private TelegramBotService bot;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            // Типа DI контейнер =)
            try
            {
                var config = BotConfiguration.GetOrCreateConfig();
                IVkService vkApi = new VkService(config);
                IDbContext dbContext = new DbContext();

                bot = new TelegramBotService(config, vkApi, dbContext);
                await Task.Run(() => bot.StartBot(cancellationToken), cancellationToken);
            }
            catch (Exception)
            {
                Console.WriteLine($"Возникли проблемы при запуске бота... \n" +
                    $"Проверьте конфигурационный файл: \n {BotConfiguration.BaseDirectory}\\{BotConfiguration.ConfigFileName}\n" +
                    $"");
            }

            if (bot?.Initialized ?? false)
                Console.WriteLine("Бот запущен успешно");
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service is stopping...");
            await base.StopAsync(cancellationToken);
            bot?.Dispose();
        }
    }
}
