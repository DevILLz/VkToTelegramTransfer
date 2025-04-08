using HotBike.Telegram.Bot.Objects;

namespace HotBike.Telegram.Bot
{
    public interface IVkApi
    {
        Task<List<Post>> CheckLatestVkPosts();
    }
}