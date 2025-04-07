//using Newtonsoft.Json;
namespace HotBike.Telegram.Bot.Objects;

public class Reactions
{
    public int Count { get; set; }
    public List<ReactionItem> Items { get; set; }
}
