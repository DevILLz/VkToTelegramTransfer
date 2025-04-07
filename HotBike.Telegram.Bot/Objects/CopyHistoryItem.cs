//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Objects;

public class CopyHistoryItem
{
    public string InnerType { get; set; }
    public string Type { get; set; }
    public List<Attachment> Attachments { get; set; }
    public long Date { get; set; }
    public long FromId { get; set; }
    public int Id { get; set; }
    public long OwnerId { get; set; }
    public PostSource PostSource { get; set; }
    public string PostType { get; set; }
    public string Text { get; set; }
}
