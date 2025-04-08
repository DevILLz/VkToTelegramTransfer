//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Services.VkResponseObjects;

public class Response
{
    public int Count { get; set; }
    public List<Post> Items { get; set; } = new List<Post>();
}
