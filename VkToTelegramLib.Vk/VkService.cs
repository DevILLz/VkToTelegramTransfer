using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VkToTelegramLib.Infrastructure;
using VkToTelegramLib.Infrastructure.Interfaces;
using VkToTelegramLib.Infrastructure.VkResponseObjects;

namespace VkToTelegramLib.Vk
{
    public class VkService(BotConfiguration config) : IVkService
    {
        private readonly BotConfiguration config = config;
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

            Console.WriteLine($"Постов после фильтрации по дате и исключения репостов: {filtredPosts.Count}");
            return filtredPosts;
        }

        private async Task<List<Post>> GetVkPostsInternal()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vkServiceKey);

            try
            {
                var countAttributeValue = config.VkRequestAttributes.FirstOrDefault(a => a.Name == "Count")?.Value;
                Console.WriteLine($"Получение последних {countAttributeValue} постов из ВК");

                HttpResponseMessage response = await client.GetAsync(GetVkPostsRequestUrl());
                response.EnsureSuccessStatusCode(); // Выбрасывает исключение, если код статуса не успешный


                // Читаем ответ как строку
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VkResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (responseObject is null)
                    throw new Exception("Не удалось распознать ответ сервера");

                Console.WriteLine($"Получено {responseObject.Response.Items.Count} постов");

                return responseObject.Response.Items;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nОшибка при выполнении запроса:");
                Console.WriteLine(e.Message);
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
            //    Console.WriteLine($"Получение последних {countAttributeValue} постов из ВК");

            //    HttpResponseMessage response = await client.GetAsync(GetVkPostsRequestUrl());
            //    response.EnsureSuccessStatusCode(); // Выбрасывает исключение, если код статуса не успешный


            //    // Читаем ответ как строку
            //    string responseBody = await response.Content.ReadAsStringAsync();
            //    var responseObject = JsonSerializer.Deserialize<VkResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            //    if (responseObject is null)
            //        throw new Exception("Не удалось распознать ответ сервера");

            //    Console.WriteLine($"Получено {responseObject.Response.Items.Count} постов");

            //    return responseObject.Response.Items;
            //}
            //catch (HttpRequestException e)
            //{
            //    Console.WriteLine("\nОшибка при выполнении запроса:");
            //    Console.WriteLine(e.Message);
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
}