using VkToTelegramLib.Infrastructure.VkResponseObjects;

namespace VkToTelegramLib.Infrastructure.Interfaces
{
    public interface IVkService
    {
        Task<List<Post>> GetLatestVkPosts();
        Task<List<object>> GetVkStories();
    }
}