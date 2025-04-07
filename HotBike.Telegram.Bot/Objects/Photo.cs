//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Objects;

public class Photo
{
    public long AlbumId { get; set; }
    public long Date { get; set; }
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string AccessKey { get; set; }
    public List<PhotoSize> Sizes { get; set; }
    public string Text { get; set; }
    public int? UserId { get; set; }
    public string WebViewToken { get; set; }
    public bool HasTags { get; set; }
    public OrigPhoto OrigPhoto { get; set; }
}
