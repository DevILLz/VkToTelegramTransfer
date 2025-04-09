using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Infrastructure.VkResponseObjects;

namespace VkToTelegramLib.Telegram;
public partial class TelegramBotService(BotConfiguration config, IVkService vkApi, IDbContext context) : IDisposable
{
    private ITelegramBotClient bot;
    private readonly IVkService vkApi = vkApi;
    private readonly IDbContext context = context;
    private readonly BotConfiguration config = config;
    private Timer checkTimer;

    public bool Initialized { get; private set; }

    public void StartBot(CancellationToken cancellationToken = default)
    {
        try
        {
            bot = new TelegramBotClient(config.TelegramToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        if (bot is null)
            throw new Exception("Telegram bot not configured");
        if (config is null)
            throw new Exception("Configuration doesn't exists");
        if (vkApi is null)
            throw new Exception("VkApi not configured");
        if (context is null)
            throw new Exception("DataBase not configured");

        //CheckPostUpdates();
        SetUpTimer();
        bot.StartReceiving(
            HandleUpdateAsync,
            HandleError,
            new ReceiverOptions { AllowedUpdates = { } },
            cancellationToken: cancellationToken
        );

        Initialized = true;
    }

    private void SetUpTimer()
    {
        var now = DateTime.Now;

        // что бы таймер срабатывал в нужное время (12:00 - 12:30 - 13:00...)
        // нужно его запустить в один из этих промежутков времени
        var interval = config.PostCheckInterval;

        var startTimerTime = ((now.Minute / interval) + 1) * interval;
        if (startTimerTime > 60) 
            startTimerTime = 60;

        var nextTrigger = new DateTime(
            now.Year, now.Month, now.Day,
            now.Hour, 0, 0
        ).AddMinutes(startTimerTime);

        var startDelay = (long)(nextTrigger - now).TotalMilliseconds;

        checkTimer = new Timer(o => Task.Run(CheckPostUpdates), null, startDelay, 1000 * 60 * interval);
    }

    private async Task CheckPostUpdates()
    {
        var latestPosts = await vkApi.GetLatestVkPosts();
        if (latestPosts is null || latestPosts.Count == 0)
            return;

        latestPosts.Reverse(); // в обратном порядке публикуем
        foreach (var post in latestPosts)
        {
            Console.WriteLine($"Проверка поста {post.Id}. Текст: {post.Text.Take(30)}...");
            var messageLink = context.GetMessageLink(post) ?? throw new Exception("Ошибка при запросе MessageLink");

            if (messageLink?.VkMessageHash is null)
                Console.WriteLine($"Данный пост еще не был опубликован");

            //context.DebugRequest();
            if (messageLink?.VkMessageHash is null) // добавление нового поста
            {
                await SendPostToGroup(post);
                continue;
            }

            if (messageLink.Edited != post.Edited && messageLink.DateTime >= DateTime.Now.AddDays(-7)) // обновление поста
            { // если пост старше недели и он уже был опубликован, то не может быть отредактирован (ограничения ВК)
                await UpdatePostToGroup(post, messageLink.TelegramMessageId);
                continue;
            }
        }
    }

    private async Task SendPostToGroup(Post vkPost)
    {
        var photo = vkPost.Attachments.FirstOrDefault(a => a.Type == "photo")?.Photo.OrigPhoto.Url;
        var telegramText = UpdateMessageForTelegramPost(vkPost);

        var telegramMessageId = 0;
        if (photo is null || vkPost.Text.Length > 1024) // если под фото больше 1024 символов, то такой пост не сделать в телеге из за ограничений, убираем фото
        {
            Console.WriteLine($"В данном посте либо нет фотографии, либо текст слишком длинный для подобного поста, публикуем как текст");

            if (vkPost.Text.Length > 4096)
                return; // TODO: разделять на несколько постов
            // или в комментарии добавлять остаток текста

            //TODO: обработать добавление фоток в постах
            telegramMessageId = (await bot.SendMessage(new ChatId(config.TelegramChatId), telegramText, ParseMode.Html)).Id;

            context.AddOrUpdatePostInDb(vkPost, telegramMessageId);
            return;
        }

        telegramMessageId = (await bot.SendPhoto(new ChatId(config.TelegramChatId), photo, telegramText, ParseMode.Html)).Id;

        context.AddOrUpdatePostInDb(vkPost, telegramMessageId);
    }

    private async Task UpdatePostToGroup(Post vkPost, int telegramMessageId)
    {
        var photo = vkPost.Attachments.FirstOrDefault(a => a.Type == "photo")?.Photo.OrigPhoto.Url;
        var telegramText = UpdateMessageForTelegramPost(vkPost);

        if (photo is null || vkPost.Text.Length > 1024) // если под фото больше 1024 символов, то такой пост не сделать в телеге из за ограничений, убираем фото
        {
            Console.WriteLine($"В данном посте либо нет фотографии, либо текст слишком длинный для подобного поста, публикуем как текст");

            if (vkPost.Text.Length > 4096)
                return; // TODO: разделять на несколько постов
                        // или в комментарии добавлять остаток текста

            Console.WriteLine($"Посты уже был опубликован, пытаемся обновить...");
            await bot.EditMessageText(new ChatId(config.TelegramChatId), telegramMessageId, telegramText, ParseMode.Html);
            Console.WriteLine($"Пост обновлен");

            context.AddOrUpdatePostInDb(vkPost, telegramMessageId);
            return;
        }

        Console.WriteLine($"Посты уже был опубликован, пытаемся обновить...");
        if (photo is not null)
        {
            // Потенциальная ошибка
            try
            { // если попытаться обновить фото в посте, где изменился только текст, то этот запрос выкинет исключение
              // и, почему-то, при обновлении картинки, стирается весь текст =(
                await bot.EditMessageMedia(new ChatId(config.TelegramChatId), telegramMessageId, new InputMediaPhoto(photo));
            }
            catch { }
            try
            { // по этому текст обновляем принудительно всегда в таких постах
                await bot.EditMessageCaption(new ChatId(config.TelegramChatId), telegramMessageId, telegramText, ParseMode.Html);
            }
            catch { }
        }
        else
            await bot.EditMessageText(new ChatId(config.TelegramChatId), telegramMessageId, telegramText);
        Console.WriteLine($"Пост обновлен");

        context.AddOrUpdatePostInDb(vkPost, telegramMessageId);
    }

    private string UpdateMessageForTelegramPost(Post vkPost)
    {
        var addiditionalInfo = $"\n\n<a href=\"{config.VkLink}\">Группа ВК</a>\n"
            + "#ГорячиеВеломаны";

        var pattern = @"\[(https?:\/\/[^\|\]]+)\|([^\]]+)\]";

        return Regex.Replace(vkPost.Text, pattern, m =>
        {
            var url = m.Groups[1].Value;

            if (config.UrlPartDificultiesVK != "undefined" && config.UrlDificultiesTG != "undefined")
                url = m.Groups[1].Value.Contains(config.UrlPartDificultiesVK)
                    ? config.UrlDificultiesTG
                    : m.Groups[1].Value;

            var text = m.Groups[2].Value;

            return $"<a href=\"{url}\">{text}</a>";
        }) + addiditionalInfo;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                Console.WriteLine($"Получено сообщение от {update.Message!.Chat}: {update.Message!.Text}");
                await OnMessage(botClient, update.Message, cancellationToken);
                break;
            case UpdateType.MyChatMember:
                if (update.MyChatMember is null)
                    break;

                Console.WriteLine($"Какие-то действия с чатом {update.MyChatMember.Chat} : {update.MyChatMember.Chat.Id}");
                break;
            case UpdateType.Unknown:
                break;
            case UpdateType.InlineQuery:
                break;
            case UpdateType.ChosenInlineResult:
                break;
            case UpdateType.CallbackQuery:
                break;
            case UpdateType.EditedMessage:
                break;
            case UpdateType.ChannelPost:
                break;
            case UpdateType.EditedChannelPost:
                break;
            case UpdateType.ShippingQuery:
                break;
            case UpdateType.PreCheckoutQuery:
                break;
            case UpdateType.Poll:
                break;
            case UpdateType.PollAnswer:
                break;
            case UpdateType.ChatMember:
                break;
            case UpdateType.ChatJoinRequest:
                break;
            case UpdateType.MessageReaction:
                break;
            case UpdateType.MessageReactionCount:
                break;
            case UpdateType.ChatBoost:
                break;
            case UpdateType.RemovedChatBoost:
                break;
            case UpdateType.BusinessConnection:
                break;
            case UpdateType.BusinessMessage:
                break;
            case UpdateType.EditedBusinessMessage:
                break;
            case UpdateType.DeletedBusinessMessages:
                break;
            case UpdateType.PurchasedPaidMedia:
                break;
            default: break;
        }
    }

    private async Task OnMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var textCommands = new string[]
        {
            "/start",
            "/update"
        };
        if (message?.Text is null)
            return;

        var inMessageButtons = new InlineKeyboardMarkup(
                [[
                    InlineKeyboardButton.WithUrl("Сообщество телеграм", config.TelegrasmLink),
                    InlineKeyboardButton.WithUrl("Сообщество ВК", config.VkLink),
                ],
            ]);

        var keyboard = new List<KeyboardButton>
        {
            new() { Text = "Проверить обновления постов" }
        };
        var inKeyboardButtons = new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>> { keyboard },
            ResizeKeyboard = true,
        };

        var culture = StringComparison.InvariantCultureIgnoreCase;
        if (message.Text.Contains(textCommands[0], culture))
        {
            var responseMessage = "Вы можете давать команды боту с помочью кнопок под клавиатурой \nКоманды бота: ";
            foreach (var command in textCommands)
                responseMessage += $"\n{command}";

            await botClient.SendMessage(message.Chat, responseMessage, cancellationToken: cancellationToken, replyMarkup: inKeyboardButtons);
        }

        if (message.Text.Contains(textCommands[1], culture) || message.Text.Contains(keyboard[0].Text, culture))
        {
            await CheckPostUpdates();
            await botClient.SendMessage(message.Chat, "Проверка обновлений завершена", cancellationToken: cancellationToken, replyMarkup: inMessageButtons);
        }
    }

    private async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        await Task.Delay(0, cancellationToken);
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

    public void Dispose()
    {
        checkTimer?.Dispose();
        bot.Close();
        Initialized = false;
    }
}