using System.Net;
using System.Net.Mail;

namespace MecaFlow2025.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("La configuración de email no está completa");
                }

                using var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, "MecaFlow 2025"),
                    Subject = "Restablecimiento de Contraseña - MecaFlow",
                    IsBodyHtml = true,
                    Body = CreateEmailBody(resetLink)
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Correo de restablecimiento enviado exitosamente a {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de restablecimiento a {Email}", toEmail);
                throw new Exception($"Error al enviar el correo: {ex.Message}");
            }
        }

        private string CreateEmailBody(string resetLink)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f4f4f4; padding: 20px;'>
                    <div style='background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #1b6ec2; margin: 0;'>🔧 MecaFlow 2025</h1>
                            <h2 style='color: #333; margin-top: 10px;'>Restablecimiento de Contraseña</h2>
                        </div>
                        
                        <div style='color: #555; line-height: 1.6;'>
                            <p>Hola,</p>
                            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en MecaFlow 2025.</p>
                            <p>Si no solicitaste este cambio, puedes ignorar este correo de manera segura.</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                    🔑 Restablecer Contraseña
                                </a>
                            </div>
                            
                            <p><strong>⚠️ Importante:</strong></p>
                            <ul style='color: #666;'>
                                <li>Este enlace es válido por 1 hora</li>
                                <li>Solo puede ser usado una vez</li>
                                <li>Si el enlace no funciona, cópialo y pégalo directamente en tu navegador</li>
                            </ul>
                            
                            <p style='font-size: 12px; color: #888; border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                                Este correo fue enviado automáticamente. Por favor, no respondas a este mensaje.<br>
                                © 2025 MecaFlow - Sistema de Gestión Automotriz
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}