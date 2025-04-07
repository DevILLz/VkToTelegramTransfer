using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using HotBike.Telegram.Bot.Objects;
using LiteDB;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;

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

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }, // receive all update types
        };

        //TODO: добавить очередь публикации и привязать к точному времени
        // сложности - публикация в вк может быть в ровное время, и как-то надо при этом в ровное время публиковать в телеге
        checkTimer = new Timer(o => Task.Run(OnTimerTrigger), null, 1000 * 60 * 60, Timeout.Infinite);

        OnTimerTrigger();
        //bot.StartReceiving(
        //    HandleUpdateAsync,
        //    HandleErrorAsync,
        //    receiverOptions
        //);
    }

    private async Task OnTimerTrigger()
    {
        var latestPosts = await vkApi.CheckLatestVkPosts();
        if (latestPosts is null || latestPosts.Count == 0)
            return;

        latestPosts.Reverse(); // в обратном порядке публикуем
        foreach (var post in latestPosts)
        {
            var messageLink = dbContext.GetMessageLink(post);
            if (messageLink?.VkMessageHash is null)
            {
                await SendPostToGroup(post);
                continue;
            }

            if (messageLink.Edited != post.Edited) // обновление поста в телеге
            {
                await SendPostToGroup(post);// update
                continue;
            }
        }
    }

    private async Task SendPostToGroup(Post vkPost)
    {
        var photo = vkPost.Attachments.FirstOrDefault(a => a.Type == "photo")?.Photo.OrigPhoto.Url;
        var vkGroupLink = "https://vk.com/goryachievelomany";
        var addiditionalInfo = $"\n\n<a href=\"{vkGroupLink}\">Группа ВК</a>\n"
            + "#ГорячиеВеломаны";
        if (vkPost.Text.Length > 1024 || photo is null)
        {
            if (vkPost.Text.Length > 4096)
                return; // TODO: разделять на несколько постов

            var tgMessage = await bot.SendMessage(new ChatId(RequestConstants.TelegramChatId), vkPost.Text + addiditionalInfo);
            dbContext.AddOrUpdatePostInDb(vkPost, tgMessage.Id);
            return;
        }

        //TODO: замена ссылок
        string urlDificultiesVK = "wall-226433411_294";
        string urlDificultiesTG = "https://t.me/HotBikeYar/89";
        string pattern = @"\[(https?:\/\/[^\|\]]+)\|([^\]]+)\]";
        string resultText = Regex.Replace(vkPost.Text, pattern, m =>
        {
            string url = m.Groups[1].Value.Contains(urlDificultiesVK) ? urlDificultiesTG : m.Groups[1].Value;
            string text = m.Groups[2].Value;
            
            return $"<a href=\"{url}\">{text}</a>";
        });

        var tgMessage1 = await bot.SendPhoto(new ChatId(RequestConstants.TelegramChatId), photo, resultText + addiditionalInfo, ParseMode.Html);

        dbContext.AddOrUpdatePostInDb(vkPost, tgMessage1.Id);
    }

    //public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    //{
    //    // Некоторые действия
    //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
    //    if (update.Type == UpdateType.Message)
    //    {
    //        var message = update.Message;
    //        if (message.Text.ToLower() == "/start")
    //        {
    //            await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
    //            return;
    //        }
    //        await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
    //    }
    //}

    //public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    //{
    //    // Некоторые действия
    //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    //}

    public void Dispose()
    {
        checkTimer.Dispose();
        bot.Close();
    }
}