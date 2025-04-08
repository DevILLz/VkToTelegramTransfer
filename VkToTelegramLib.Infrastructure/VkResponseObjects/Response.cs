//using Newtonsoft.Json;
namespace VkToTelegramLib.Infrastructure.VkResponseObjects;

public class Response
{
    public int Count { get; set; }
    public List<Post> Items { get; set; } = new List<Post>();
}
