using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecaFlow2025.Models
{
    public partial class Diagnostico
    {
        [Key]
        public int DiagnosticoId { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El detalle es obligatorio.")]
        [StringLength(500, ErrorMessage = "El detalle no puede exceder los 500 caracteres.")]
        public string Detalle { get; set; } = null!;

        [Required(ErrorMessage = "El empleado es obligatorio.")]
        public int EmpleadoId { get; set; }

        [ForeignKey("EmpleadoId")]
        [InverseProperty("Diagnosticos")]
        public virtual Empleado? Empleado { get; set; } = null!;

        [ForeignKey("VehiculoId")]
        [InverseProperty("Diagnosticos")]
        public virtual Vehiculo? Vehiculo { get; set; } = null!;
    }
}
