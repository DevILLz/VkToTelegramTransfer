//using Newtonsoft.Json;
namespace VkToTelegramLib.Infrastructure.VkResponseObjects;

public class Post
{
    public string InnerType { get; set; }
    public Donut Donut { get; set; }
    public int? IsPinned { get; set; }
    public Comments Comments { get; set; }
    public int MarkedAsAds { get; set; }
    public string Hash { get; set; }
    public string Type { get; set; }
    public PushSubscription PushSubscription { get; set; }
    public List<Attachment> Attachments { get; set; }
    public long Date { get; set; }
    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Date)
                                        .ToLocalTime().DateTime;
    public long? Edited { get; set; }
    public long FromId { get; set; }
    public int Id { get; set; }
    public Likes Likes { get; set; }
    public string ReactionSetId { get; set; }
    public Reactions Reactions { get; set; }
    public long OwnerId { get; set; }
    public PostSource PostSource { get; set; }
    public string PostType { get; set; }
    public Reposts Reposts { get; set; }
    public string Text { get; set; }
    public Views Views { get; set; }
    public int? CarouselOffset { get; set; }
    public bool? CheckSign { get; set; }
    public List<CopyHistoryItem> CopyHistory { get; set; }
}
