//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Objects;

public class Response
{
    public int Count { get; set; }
    public List<Post> Items { get; set; } = new List<Post>();
}
