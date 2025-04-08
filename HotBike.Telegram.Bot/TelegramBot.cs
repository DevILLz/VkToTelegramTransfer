using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using HotBike.Telegram.Bot.Objects;
using LiteDB;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HotBike.Telegram.Bot;
public partial class TelegramBot : IDisposable
{
    private string telegramToken;
    private ITelegramBotClient bot;
    private VkApi vkApi;
    private DbContext dbContext;
    private Timer checkTimer;

    public TelegramBot()
    {
        telegramToken = File.ReadAllText(RequestConstants.BaseDirectory + "/tgToken.txt");
    }

    public void StartBot()
    {
        bot = new TelegramBotClient(telegramToken);
        vkApi = new VkApi();
        dbContext = new DbContext();

        //CheckPostUpdates();
        SetUpTimer();
        bot.StartReceiving(
            HandleUpdateAsync,
            HandleError,
            new ReceiverOptions { AllowedUpdates = { } }
        );
    }

    private void SetUpTimer()
    {
        var now = DateTime.Now;

        // что бы таймер срабатывал в нужное время (12:00 - 12:30 - 13:00...)
        // нужно его запустить в один из этих промежутков времени

        var minutes = now.Minute < 30 ? 30 : 60;

        var nextTrigger = new DateTime(
            now.Year, now.Month, now.Day,
            now.Hour, 0, 0
        ).AddMinutes(minutes);

        var interval = (long)(nextTrigger - now).TotalMilliseconds;

        checkTimer = new Timer(o => Task.Run(CheckPostUpdates), null, interval, 1000 * 60 * 30);
    }

    private async Task CheckPostUpdates()
    {
        var latestPosts = await vkApi.CheckLatestVkPosts(RequestConstants.StartCheckDate);
        if (latestPosts is null || latestPosts.Count == 0)
            return;

        latestPosts.Reverse(); // в обратном порядке публикуем
        foreach (var post in latestPosts)
        {
            var messageLink = dbContext.GetMessageLink(post);
            if (messageLink?.VkMessageHash is null || (messageLink.Edited != post.Edited && messageLink.DateTime >= DateTime.Now.AddDays(-7)))
            { // если пост старше недели и он уже был опубликован, то не может быть отредактирован (ограничения ВК)
                await SendOrUpdatePostToGroup(post, messageLink?.TelegramMessageId);
                continue;
            }
        }
    }

    private async Task SendOrUpdatePostToGroup(Post vkPost, int? telegramMessageId = null)
    {
        var photo = vkPost.Attachments.FirstOrDefault(a => a.Type == "photo")?.Photo.OrigPhoto.Url;
        var vkGroupLink = "https://vk.com/goryachievelomany";
        var addiditionalInfo = $"\n\n<a href=\"{vkGroupLink}\">Группа ВК</a>\n"
            + "#ГорячиеВеломаны";

        if (photo is null || vkPost.Text.Length > 1024)
        {
            if (vkPost.Text.Length > 4096)
                return; // TODO: разделять на несколько постов
            // или в комментарии добавлять остаток текста

            //TODO: обработать добавление фоток в постах
            if (telegramMessageId is null || telegramMessageId == 0)
                telegramMessageId = (await bot.SendMessage(new ChatId(RequestConstants.TelegramChatId), vkPost.Text + addiditionalInfo)).Id;
            else
                await bot.EditMessageText(new ChatId(RequestConstants.TelegramChatId), telegramMessageId.Value, vkPost.Text + addiditionalInfo);

            dbContext.AddOrUpdatePostInDb(vkPost, telegramMessageId.Value);
            return;
        }

        //TODO: замена ссылок на пост с уровнями сложности        
        string pattern = @"\[(https?:\/\/[^\|\]]+)\|([^\]]+)\]";
        string resultText = Regex.Replace(vkPost.Text, pattern, m =>
        {
            string url = m.Groups[1].Value.Contains(RequestConstants.UrlPartDificultiesVK) 
            ? RequestConstants.UrlDificultiesTG 
            : m.Groups[1].Value;

            string text = m.Groups[2].Value;
            
            return $"<a href=\"{url}\">{text}</a>";
        });

        if (telegramMessageId is null || telegramMessageId == 0)
            telegramMessageId = (await bot.SendPhoto(new ChatId(RequestConstants.TelegramChatId), photo, resultText + addiditionalInfo, ParseMode.Html)).Id;
        else
        {
            if (photo is not null)
            {
                await bot.EditMessageCaption(new ChatId(RequestConstants.TelegramChatId), telegramMessageId.Value, vkPost.Text + addiditionalInfo);

                // Потенциальная ошибка
                await bot.EditMessageMedia(new ChatId(RequestConstants.TelegramChatId), telegramMessageId.Value, new InputMediaPhoto(photo));
            }
            else
                await bot.EditMessageText(new ChatId(RequestConstants.TelegramChatId), telegramMessageId.Value, vkPost.Text + addiditionalInfo);
        }
        
        dbContext.AddOrUpdatePostInDb(vkPost, telegramMessageId.Value);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await OnMessage(botClient, update.Message, cancellationToken);
                Console.WriteLine($"Получено сообщение от {update.Message!.Chat}: {update.Message!.Text}");
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

    private async Task OnMessage(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
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
                    InlineKeyboardButton.WithUrl("Сообщество телеграм", "https://t.me/HotBikeYar"),
                    InlineKeyboardButton.WithUrl("Сообщество ВК", "https://vk.com/goryachievelomany"),
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

        if (string.Compare(message.Text, textCommands[0], true) == 0)
        {
            var responseMessage = "Вы можете давать команды боту с помочью кнопок под клавиатурой \nКоманды бота: ";
            foreach (var command in textCommands)
                responseMessage += $"\n{command}";

            await botClient.SendMessage(message.Chat, responseMessage, cancellationToken: cancellationToken, replyMarkup: inKeyboardButtons);
        }

        if (string.Compare(message.Text, textCommands[1], true) == 0 || string.Compare(message.Text, keyboard[0].Text, true) == 0)
        {
            await CheckPostUpdates();
            await botClient.SendMessage(message.Chat, "Проверка обновлений завершена", cancellationToken: cancellationToken, replyMarkup: inMessageButtons);
        }
    }

    private async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

    public void Dispose()
    {
        checkTimer.Dispose();
        bot.Close();
    }
}