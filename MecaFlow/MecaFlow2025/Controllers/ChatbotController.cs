using Microsoft.AspNetCore.Mvc;
using MecaFlow2025.Services;
using MecaFlow2025.Attributes;
using MecaFlow2025.Models;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador", "Empleado", "Cliente")]
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbotService;

        public ChatbotController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] ChatMessage request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { response = "Por favor escribe tu pregunta" });
                }

                var response = _chatbotService.GetResponse(request.Message);
                return Json(new { response });
            }
            catch (Exception ex)
            {
                // Log the error properly here
                Console.WriteLine($"Error in chatbot: {ex.Message}");
                return Json(new { response = "Ocurrió un error al procesar tu pregunta. Por favor intenta nuevamente." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}