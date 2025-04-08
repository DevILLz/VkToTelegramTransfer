//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Services.VkResponseObjects;

public class Comments
{
    public int CanPost { get; set; }
    public int CanView { get; set; }
    public int Count { get; set; }
    public bool GroupsCanPost { get; set; }
}
