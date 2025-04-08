using HotBike.Telegram.Bot.Services.VkResponseObjects;

namespace HotBike.Telegram.Bot.Interfaces
{
    public interface IVkApi
    {
        Task<List<Post>> CheckLatestVkPosts();
    }
}