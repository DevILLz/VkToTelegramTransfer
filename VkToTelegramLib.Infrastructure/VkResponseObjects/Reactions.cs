//using Newtonsoft.Json;
namespace VkToTelegramLib.Infrastructure.VkResponseObjects;

public class Reactions
{
    public int Count { get; set; }
    public List<ReactionItem> Items { get; set; }
}
