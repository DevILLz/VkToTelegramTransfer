//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Objects;

public class Attachment
{
    public string Type { get; set; }
    public Photo Photo { get; set; }
    public Audio Audio { get; set; }
}
