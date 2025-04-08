using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HotBike.Telegram.Bot;

public class BotConfiguration
{
    public static string BaseDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\HotBike";
    public static string ConfigFileName { get; set; } = "configuration.cfg";
    public string TelegramToken { get; set; }
    public string VkToken { get; set; }
    public long TelegramChatId { get; set; }
    public string VkGetPostsUrl { get; set; }
    public DateTime StartCheckDate { get; set; }

    // specific for this bot
    public string UrlPartDificultiesVK { get; set; }
    public string UrlDificultiesTG { get; set; }

    public List<VkRequestAttribute> VkRequestAttributes { get; set; }

    internal static BotConfiguration GetDefaults()
    {
        return new BotConfiguration
        {
            TelegramToken = "undefined",
            VkToken = "undefined",

            VkRequestAttributes = 
            [
                new VkRequestAttribute("Version", "v", "5.199"),
                new VkRequestAttribute("Domain", "domain", "goryachievelomany"),
                new VkRequestAttribute("Filter", "filter", "owner"),
                new VkRequestAttribute("Count", "count", "15"),
            ],
            TelegramChatId = -1002484373576, // -1002540846422 - test
            VkGetPostsUrl = "https://api.vk.com/method/wall.get",
            StartCheckDate = DateTime.Parse("07.04.25"),

            UrlPartDificultiesVK = "wall-226433411_294",
            UrlDificultiesTG = "https://t.me/HotBikeYar/89",
        };
    }
    public static BotConfiguration GetOrCreateConfig()
    {
        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);

        var configFilePath = BaseDirectory + "/" + ConfigFileName;
        BotConfiguration config;

        if (!File.Exists(configFilePath))
        {
            config = GetDefaults();
            File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true // <-- делает JSON читаемым
            }));

            return config;
        }

        try
        {
            var configFile = File.ReadAllText(configFilePath);
            config = JsonSerializer.Deserialize<BotConfiguration>(configFile)!;

            return config;
        }
        catch (Exception e)
        {
            Console.WriteLine("Can not read configuration file: ", e.Message);
            return GetDefaults();
        }
    }
    public record VkRequestAttribute(string Name, string Atribute, string Value);
}