using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NvnDesk.Application.Interfaces;

namespace NvnDesk.Infrastructure.Services
{
    
    public class GeminiAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

       
        public GeminiAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> SummarizeTicketAsync(string title, string description)
        {
            
            var prompt = $"Aşağıdaki destek talebini tek cümlede, Türkçe olarak özetle:\n\nBaşlık: {title}\nAçıklama: {description}";

            var responseText = await CallGeminiAsync(prompt);
            return responseText.Trim();
        }

        public async Task<(string Category, string Priority)> PredictCategoryAndPriorityAsync(string title, string description)
        {
            
            
            var prompt = $@"Aşağıdaki destek talebini analiz et. Kategori olarak sadece şunlardan birini seç: Technical, Billing, General.
Öncelik olarak sadece şunlardan birini seç: Low, Medium, High.

Başlık: {title}
Açıklama: {description}

Cevabı SADECE şu JSON formatında ver, başka hiçbir metin ekleme:
{{""category"": ""..."", ""priority"": ""...""}}";

            var responseText = await CallGeminiAsync(prompt);

            
            var cleanJson = responseText.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(cleanJson);
            var category = doc.RootElement.GetProperty("category").GetString() ?? "General";
            var priority = doc.RootElement.GetProperty("priority").GetString() ?? "Medium";

            return (category, priority);
        }

        
        
        private async Task<string> CallGeminiAsync(string prompt)
        {
            var apiKey = _configuration["AISettings:ApiKey"];
            var model = _configuration["AISettings:Model"];

            
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode(); 

            var responseBody = await response.Content.ReadAsStringAsync();

            
            
            using var responseDoc = JsonDocument.Parse(responseBody);
            var text = responseDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}