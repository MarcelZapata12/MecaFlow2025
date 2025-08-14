
// IEmailService.cs
namespace MecaFlow2025.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
