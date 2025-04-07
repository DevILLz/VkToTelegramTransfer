namespace HotBike.Telegram.Bot;
public class VkToTgMessage
{
    public Guid Id { get; set; }
    public string VkMessageHash { get; set; }
    public long Date { get; set; }
    public long? Edited { get; set; }
    public long TelegramMessageId { get; set; }
}
