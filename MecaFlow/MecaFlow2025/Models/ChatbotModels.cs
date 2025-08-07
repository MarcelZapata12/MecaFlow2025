namespace MecaFlow2025.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ChatbotResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
    }

    // Clase para recibir el request del frontend
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}