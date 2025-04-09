using System.Text.Json;
namespace VkToTelegramLib.Infrastructure;

public class BotConfiguration
{
    public static string BaseDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VkToTelegram";
    public static string ConfigFileName { get; set; } = "configuration.cfg";
    public string TelegramToken { get; set; }
    public string VkServiceKey { get; set; }
    public string VkGroupKey { get; set; }
    public long TelegramChatId { get; set; }
    public string VkGetPostsUrl { get; set; }
    public string TelegrasmLink { get; set; }
    public string VkLink { get; set; }
    public DateTime StartCheckDate { get; set; }
    public int PostCheckInterval { get; set; } // в минутах

    // specific for this bot
    public string UrlPartDificultiesVK { get; set; }
    public string UrlDificultiesTG { get; set; }

    public List<VkRequestAttribute> VkRequestAttributes { get; set; }

    internal static BotConfiguration GetDefaults()
    {
        return new BotConfiguration
        {
            TelegramToken = "undefined",
            VkServiceKey = "undefined",
            VkGroupKey = "undefined",

            VkRequestAttributes =
            [
                new VkRequestAttribute("Version", "v", "5.199"),
                new VkRequestAttribute("Filter", "filter", "owner"),
                new VkRequestAttribute("Count", "count", "15"),
                new VkRequestAttribute("Domain", "domain", "undefined"),
            ],
            VkGetPostsUrl = "https://api.vk.com/method/wall.get",
            StartCheckDate = DateTime.Parse("07.04.25"),

            TelegramChatId = 0,
            UrlPartDificultiesVK = "undefined",
            UrlDificultiesTG = "undefined",
            TelegrasmLink = "undefined",
            VkLink = "undefined",
            PostCheckInterval = 30,
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