//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Services.VkResponseObjects;

public class Attachment
{
    public string Type { get; set; }
    public Photo Photo { get; set; }
    public Audio Audio { get; set; }
}
