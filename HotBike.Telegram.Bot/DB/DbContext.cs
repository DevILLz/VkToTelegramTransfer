using HotBike.Telegram.Bot.Objects;
using LiteDB;

namespace HotBike.Telegram.Bot;

public class DbContext
{
    private string fullDbPath = RequestConstants.BaseDirectory + "/HotBikeBot.db";
    public DbContext()
    {
        if (!Directory.Exists(RequestConstants.BaseDirectory))
            Directory.CreateDirectory(RequestConstants.BaseDirectory);
    }

    public void AddOrUpdatePostInDb(Post post, int tgMessageId)
    {
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
                        //Date = DateTimeOffset.FromUnixTimeSeconds(post.Date).DateTime,
                        //Edited = post.Edited is not null ? DateTimeOffset.FromUnixTimeSeconds(post.Edited.Value).DateTime : null,
                        TelegramMessageId = tgMessageId
                    });
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }

    public VkToTgMessage GetMessageLink(Post post)
    {

        var fullDbPath = RequestConstants.BaseDirectory + "/HotBikeBot.db";

        if (!Directory.Exists(RequestConstants.BaseDirectory))
            Directory.CreateDirectory(RequestConstants.BaseDirectory);

        try
        {
            using (var db = new LiteDatabase(fullDbPath))
            {
                // Get a collection (or create, if doesn't exist)
                var collection = db.GetCollection<VkToTgMessage>("VkToTgMessage");

                return collection.FindOne(x => x.VkMessageHash == post.Hash) ?? new VkToTgMessage();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new VkToTgMessage();
        }
    }
}