﻿using HotBike.Telegram.Bot.Objects;
using LiteDB;

namespace HotBike.Telegram.Bot;

public class DbContext
{
    private string fullDbPath = BotConfiguration.BaseDirectory + "/HotBikeBot.db";
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