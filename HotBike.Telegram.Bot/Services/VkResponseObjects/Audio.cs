//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Services.VkResponseObjects;

public class Audio
{
    public string Artist { get; set; }
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string Title { get; set; }
    public int Duration { get; set; }
    public bool IsExplicit { get; set; }
    public bool IsFocusTrack { get; set; }
    public string TrackCode { get; set; }
    public string Url { get; set; }
    public int StreamDuration { get; set; }
    public long Date { get; set; }
    public long AlbumId { get; set; }
    public List<MainArtist> MainArtists { get; set; }
    public bool ShortVideosAllowed { get; set; }
    public bool StoriesAllowed { get; set; }
    public bool StoriesCoverAllowed { get; set; }
    public string ReleaseAudioId { get; set; }
}
