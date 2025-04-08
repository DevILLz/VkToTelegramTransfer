﻿namespace HotBike.Telegram.Bot;

internal class RequestConstants
{
    internal record RequestAttribute(string Name, string Atribute, string DefaultValue);
    internal static string BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/HotBike";

    //internal static long TelegramChatId = -1002484373576; // main
    internal static long TelegramChatId = -1002540846422; // test
    internal static string VkGetPostsUrl = "https://api.vk.com/method/wall.get";
    internal static DateTime StartCheckDate = DateTime.Parse("07.04.25");

    internal static string UrlPartDificultiesVK = "wall-226433411_294";
    internal static string UrlDificultiesTG = "https://t.me/HotBikeYar/89";

    internal static string VkVersionAttribute = "v"; 
    internal static string VkVersionAttributeValue = "5.199"; 

    internal static string VkDomainAttribute = "domain";
    internal static string VkDomainAttributeValue = "goryachievelomany";

    internal static string VkFilterAttribute = "filter"; 
    internal static string VkFilterAttributeValue = "owner"; 

    internal static string VkCountAttribute = "count"; 
    internal static string VkCountAttributeValue = "15"; 

    internal static RequestAttribute[] VkRequestAttributes =
    [
        new RequestAttribute(nameof(VkVersionAttribute), VkVersionAttribute, VkVersionAttributeValue),
        new RequestAttribute(nameof(VkDomainAttribute), VkDomainAttribute, VkDomainAttributeValue),        
        new RequestAttribute(nameof(VkFilterAttribute), VkFilterAttribute, VkFilterAttributeValue),        
        new RequestAttribute(nameof(VkCountAttribute), VkCountAttribute, VkCountAttributeValue),        
    ];
}