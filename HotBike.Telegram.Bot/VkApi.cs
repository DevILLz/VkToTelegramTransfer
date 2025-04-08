using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HotBike.Telegram.Bot.Objects;

namespace HotBike.Telegram.Bot
{
    public class VkApi
    {
        private string vkToken;

        public VkApi()
        {
            vkToken = File.ReadAllText(RequestConstants.BaseDirectory + "/vkToken.txt");
        }
        public async Task<List<Post>> CheckLatestVkPosts(DateTime startCheckDate)
        {
            var posts = await GetLatestVkPosts();
            if (posts == null) return [];

            var filtredPosts = posts
                .Where(p => p.DateTime >= startCheckDate) // скипаем все посты до нужной даты
                .Where(p => p.CopyHistory is null) // скипаем все репосты
                ;

            return [.. filtredPosts];
        }

        private async Task<List<Post>> GetLatestVkPosts()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vkToken);

            try
            {
                HttpResponseMessage response = await client.GetAsync(GetVkPostsRequestUrl());
                response.EnsureSuccessStatusCode(); // Выбрасывает исключение, если код статуса не успешный

                // Читаем ответ как строку
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VkResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (responseObject is null)
                    throw new Exception("Не удалось распознать ответ сервера");

                return responseObject.Response.Items;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nОшибка при выполнении запроса:");
                Console.WriteLine(e.Message);
            }

            return [];
        }

        private string? GetVkPostsRequestUrl()
        {
            var request = new StringBuilder(RequestConstants.VkGetPostsUrl);

            if (RequestConstants.VkRequestAttributes.Length > 0)
                request.Append('?');

            foreach (var item in RequestConstants.VkRequestAttributes)
                request.Append($"{item.Atribute}={item.DefaultValue}&");

            request.Remove(request.Length - 1, 1);

            return request.ToString();
        }
    }
}