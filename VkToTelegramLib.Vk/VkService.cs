using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Infrastructure.VkResponseObjects;

namespace VkToTelegramLib.Vk;

public class VkService(BotConfiguration config, ILogger<VkService> logger) : IVkService
{
    private readonly BotConfiguration config = config;
    private readonly ILogger<VkService> logger = logger;
    private string vkServiceKey = config.VkServiceKey;
    private string vkGroupKey = config.VkGroupKey;

    public async Task<List<Post>> GetLatestVkPosts()
    {
        var posts = await GetVkPostsInternal();
        if (posts == null) return [];

        var filtredPosts = posts
            .Where(p => p.DateTime >= config.StartCheckDate) // скипаем все посты до нужной даты
            .Where(p => p.CopyHistory is null) // скипаем все репосты
            .ToList();

        logger.LogInformation($"Постов после фильтрации по дате и исключения репостов: {filtredPosts.Count}");
        return filtredPosts;
    }

    private async Task<List<Post>> GetVkPostsInternal()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vkServiceKey);

        try
        {
            var countAttributeValue = config.VkRequestAttributes.FirstOrDefault(a => a.Name == "Count")?.Value;
            logger.LogInformation($"Получение последних {countAttributeValue} постов из ВК");

            var response = await client.GetAsync(GetVkPostsRequestUrl());
            response.EnsureSuccessStatusCode(); // Выбрасывает исключение, если код статуса не успешный

            // Читаем ответ как строку
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<VkResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (responseObject is null)
                throw new Exception("Не удалось распознать ответ сервера");

            logger.LogInformation($"Получено {responseObject.Response.Items.Count} постов");

            return responseObject.Response.Items;
        }
        catch (HttpRequestException e)
        {
            logger.LogInformation("\nОшибка при выполнении запроса:");
            logger.LogInformation(e.Message);
        }

        return [];
    }

    public async Task<List<object>> GetVkStories()
    {
        //var client = new HttpClient();
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vkServiceKey);

        //try
        //{
        //    var countAttributeValue = config.VkRequestAttributes.FirstOrDefault(a => a.Name == "Count")?.Value;
        //    logger.LogInformation($"Получение последних {countAttributeValue} постов из ВК");

        //    HttpResponseMessage response = await client.GetAsync(GetVkPostsRequestUrl());
        //    response.EnsureSuccessStatusCode(); // Выбрасывает исключение, если код статуса не успешный

        //    // Читаем ответ как строку
        //    string responseBody = await response.Content.ReadAsStringAsync();
        //    var responseObject = JsonSerializer.Deserialize<VkResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        //    if (responseObject is null)
        //        throw new Exception("Не удалось распознать ответ сервера");

        //    logger.LogInformation($"Получено {responseObject.Response.Items.Count} постов");

        //    return responseObject.Response.Items;
        //}
        //catch (HttpRequestException e)
        //{
        //    logger.LogInformation("\nОшибка при выполнении запроса:");
        //    logger.LogInformation(e.Message);
        //}

        await Task.Delay(0);
        return [];
    }

    private string GetVkPostsRequestUrl()
    {
        var request = new StringBuilder(config.VkGetPostsUrl);

        if (config.VkRequestAttributes.Count > 0)
            request.Append('?');

        foreach (var item in config.VkRequestAttributes)
            request.Append($"{item.Atribute}={item.Value}&");

        request.Remove(request.Length - 1, 1);

        return request.ToString();
    }
}