using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecaFlow2025.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Token { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; } = false;

        // Navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        // Método para verificar si el token está válido
        public bool IsValid()
        {
            return !Usado && DateTime.Now < FechaExpiracion;
        }
    }
}