using LiteDB;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Infrastructure.VkResponseObjects;
using Microsoft.Extensions.Logging;

namespace VkToTelegramLib.Db;

public class DbContext(ILogger<DbContext> logger) : IDbContext
{
    private readonly ILogger<DbContext> logger = logger;
    private string fullDbPath = BotConfiguration.BaseDirectory + "\\VkToTeleramBot.db";
    public void AddOrUpdatePostInDb(Post post, int tgMessageId)
    {
        logger.LogInformation("Публикация в телеграмме прошла успешно, отправляем в БД");
        try
        {
            using (var db = new LiteDatabase(fullDbPath))
            {
                // Get a collection (or create, if doesn't exist)
                var collection = db.GetCollection<VkToTgMessage>("VkToTgMessage");

                var postInDb = collection.FindOne(x => x.VkMessageHash == post.Hash);

                if (postInDb is not null)
                {
                    postInDb.Edited = post.Edited;
                    collection.Update(postInDb);
                    return;
                }

                if (postInDb is null)
                {
                    collection.Insert(new VkToTgMessage
                    {
                        VkMessageHash = post.Hash,
                        Date = post.Date,
                        Edited = post.Edited,
                        TelegramMessageId = tgMessageId
                    });
                }
            }

            logger.LogInformation("Бд обновлена");
        }
        catch (Exception e)
        {
            logger.LogInformation("Ошибка при обновлении БД: ", e.Message);
        }
    }

    public VkToTgMessage GetMessageLink(Post post)
    {
        try
        {
            using var db = new LiteDatabase(fullDbPath);
            // Get a collection (or create, if doesn't exist)
            var collection = db.GetCollection<VkToTgMessage>("VkToTgMessage");

            return collection.FindOne(x => x.VkMessageHash == post.Hash) ?? new VkToTgMessage();
        }
        catch (Exception e)
        {
            logger.LogInformation(e.Message);
            return new VkToTgMessage();
        }
    }

    public void DebugRequest()
    {
        try
        {
            using var db = new LiteDatabase(fullDbPath);
            // Get a collection (or create, if doesn't exist)
            var collection = db.GetCollection<VkToTgMessage>("VkToTgMessage");

            collection.DeleteMany(x => x.VkMessageHash == "pS__-5rKKXncOUhLPQ");
        }
        catch (Exception e)
        {
            logger.LogInformation(e.Message);
        }
    }
}