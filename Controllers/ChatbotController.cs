using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FashionShop.Services;
using FashionShop.Models.ViewModels;

namespace FashionShop.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbotService;

        public ChatbotController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageViewModel message)
        {
            try
            {
                if (string.IsNullOrEmpty(message?.Message))
                {
                    return BadRequest("Tin nhắn không hợp lệ");
                }

                // Lấy user ID từ session nếu đã đăng nhập
                int? userId = HttpContext.Session.GetInt32("UserId");

                var response = await _chatbotService.GetChatbotResponseAsync(message.Message, userId, message.ChatHistory);

                return Json(new { success = true, message = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult ChatWidget()
        {
            return PartialView("_ChatWidget");
        }
    }
}