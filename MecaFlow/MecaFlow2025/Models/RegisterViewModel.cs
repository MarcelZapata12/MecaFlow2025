using System.ComponentModel.DataAnnotations;

namespace MecaFlow2025.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        [RegularExpression(@"^[0-9-]+$", ErrorMessage = "La cédula solo puede contener números y guiones")]
        public string Cedula { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [RegularExpression(@"^[0-9-\s\(\)\+]+$", ErrorMessage = "El teléfono solo puede contener números, espacios, paréntesis, guiones y el símbolo +")]
        public string Telefono { get; set; } = null!;


        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = null!;

        [Required(ErrorMessage = "Confirmar contraseña es obligatorio")]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = null!;
    }
}
