//using Newtonsoft.Json;
namespace VkToTelegramLib.Infrastructure.VkResponseObjects;

public class Attachment
{
    public string Type { get; set; }
    public Photo Photo { get; set; }
    public Audio Audio { get; set; }
}
