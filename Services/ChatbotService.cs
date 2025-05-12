using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FashionShop.Models.Entities;

namespace FashionShop.Services
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly double _temperature;
        private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly FashionShopContext _context;

        public ChatbotService(IHttpClientFactory httpClientFactory, IConfiguration configuration, FashionShopContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
            _maxTokens = configuration.GetValue<int>("OpenAI:MaxTokens", 500);
            _temperature = configuration.GetValue<double>("OpenAI:Temperature", 0.7);
            _context = context;
        }

        public async Task<string> GetChatbotResponseAsync(string userMessage, int? userId = null, string chatHistory = "")
        {
            try
            {
                // Tìm thông tin sản phẩm để cung cấp cho chatbot
                var productInfo = await GetProductInfoAsync(userMessage);

                // Tạo tin nhắn hệ thống để định hướng chatbot
                string systemMessage = "Bạn là trợ lý ảo cho cửa hàng thời trang FashionShop. " +
                    "Hãy giúp khách hàng tìm kiếm sản phẩm, tư vấn về quần áo, kích cỡ và phong cách thời trang. " +
                    "Trả lời ngắn gọn, thân thiện và hữu ích bằng tiếng Việt. " +
                    "Nếu khách hỏi về sản phẩm cụ thể, hãy đề xuất từ danh mục sản phẩm.";

                if (!string.IsNullOrEmpty(productInfo))
                {
                    systemMessage += $"\n\nSản phẩm hiện có trong shop:\n{productInfo}";
                }

                // Chuẩn bị dữ liệu cho API
                var requestData = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = systemMessage },
                        new { role = "user", content = userMessage }
                    },
                    max_tokens = _maxTokens,
                    temperature = _temperature
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);

                string botResponse = responseObject?.choices?[0]?.message?.content?.Trim() ??
                    "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.";

                return botResponse;
            }
            catch (Exception ex)
            {
                return $"Xin lỗi, đã xảy ra lỗi: {ex.Message}";
            }
        }

        private async Task<string> GetProductInfoAsync(string query)
        {
            try
            {
                // Phân tích truy vấn để tìm từ khóa liên quan đến danh mục sản phẩm
                var keywords = ExtractKeywords(query.ToLower());
                if (!keywords.Any()) return string.Empty;

                // Tìm kiếm sản phẩm dựa trên từ khóa
                var products = await _context.Product
                    .Include(p => p.Category)
                    .Where(p => p.stock > 0 && keywords.Any(k =>
                        p.name.Contains(k) ||
                        p.description.Contains(k) ||
                        (p.Category != null && p.Category.name.Contains(k))))
                    .Take(3)
                    .ToListAsync();

                if (!products.Any()) return string.Empty;

                // Tạo danh sách sản phẩm gợi ý
                var sb = new StringBuilder();
                foreach (var product in products)
                {
                    sb.AppendLine($"- {product.name} ({product.price:N0} VNĐ)");
                    if (!string.IsNullOrEmpty(product.description))
                    {
                        var shortDesc = product.description.Length > 100
                            ? product.description.Substring(0, 100) + "..."
                            : product.description;
                        sb.AppendLine($"  Mô tả: {shortDesc}");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private List<string> ExtractKeywords(string query)
        {
            // Danh sách từ khóa liên quan đến các danh mục sản phẩm
            var categoryKeywords = new Dictionary<string, List<string>>
            {
                { "áo", new List<string> { "áo", "thun", "khoác", "sơ mi" } },
                { "quần", new List<string> { "quần", "âu", "jean", "kaki" } },
                { "váy", new List<string> { "váy", "đầm" } },
                { "bộ", new List<string> { "bộ", "set", "đồ bộ" } }
            };

            // Tách câu truy vấn thành các từ
            var words = query.ToLower()
                .Replace(",", " ")
                .Replace(".", " ")
                .Replace("?", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // Tìm các từ khóa liên quan
            var keywords = new List<string>();
            foreach (var word in words)
            {
                foreach (var category in categoryKeywords)
                {
                    if (category.Value.Any(k => word.Contains(k)))
                    {
                        keywords.Add(category.Key);
                        break;
                    }
                }
            }

            // Thêm từ màu sắc
            var colors = new[] { "đen", "trắng", "đỏ", "xanh", "vàng", "hồng", "xám", "nâu" };
            foreach (var word in words)
            {
                if (colors.Any(c => word.Contains(c)))
                {
                    keywords.Add(word);
                }
            }

            return keywords.Distinct().ToList();
        }
    }

    // Lớp để phân tích cú pháp phản hồi JSON từ OpenAI
    public class OpenAIResponse
    {
        public Choice[] choices { get; set; }

        public class Choice
        {
            public Message message { get; set; }
        }

        public class Message
        {
            public string content { get; set; }
        }
    }
}