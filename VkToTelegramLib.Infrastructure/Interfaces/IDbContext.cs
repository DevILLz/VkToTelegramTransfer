using VkToTelegramLib.Infrastructure.VkResponseObjects;

namespace VkToTelegramLib.Infrastructure.Interfaces
{
    public interface IDbContext
    {
        void AddOrUpdatePostInDb(Post post, int tgMessageId);
        VkToTgMessage GetMessageLink(Post post);
    }
}