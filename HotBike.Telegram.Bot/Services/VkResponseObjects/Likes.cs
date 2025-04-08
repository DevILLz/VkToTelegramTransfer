//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Services.VkResponseObjects;

public class Likes
{
    public int CanLike { get; set; }
    public int Count { get; set; }
    public int UserLikes { get; set; }
    public int CanPublish { get; set; }
    public bool RepostDisabled { get; set; }
}
