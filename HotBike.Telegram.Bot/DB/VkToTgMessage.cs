namespace HotBike.Telegram.Bot;
public class VkToTgMessage
{
    public Guid Id { get; set; }
    public string VkMessageHash { get; set; }
    public long Date { get; set; }
    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Date)
                                        .ToLocalTime().DateTime;
    public long? Edited { get; set; }
    public int TelegramMessageId { get; set; }
}
