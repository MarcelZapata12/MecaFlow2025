using System.ComponentModel.DataAnnotations;

namespace MecaFlow2025.Models
{
    public class CreateEmpleadoViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚÑáéíóúñ ]+$",
            ErrorMessage = "El nombre solo debe contener letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20)]
        [RegularExpression(@"^\d+$", ErrorMessage = "La cédula debe contener solo números.")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$",
            ErrorMessage = "El correo debe contener @ y terminar en .com.")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Puesto { get; set; }

        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar contraseña es obligatorio")]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}