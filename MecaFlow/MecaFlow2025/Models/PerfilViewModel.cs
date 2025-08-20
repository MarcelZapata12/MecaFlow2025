namespace MecaFlow2025.Models
{
    public class PerfilViewModel
    {
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "";

        // Datos cliente
        public string? Telefono { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public string? Direccion { get; set; }

        // Datos empleado
        public string? Cedula { get; set; }
        public string? Puesto { get; set; }
        public DateOnly? FechaIngreso { get; set; }
        public bool? Activo { get; set; }

        // Usuario
        public DateTime? FechaCreacion { get; set; }
    }
}
