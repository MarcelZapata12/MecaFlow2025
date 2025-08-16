using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

[Index("Cedula", Name = "UQ__Empleado__B4ADFE387E911EEB", IsUnique = true)]
public partial class Empleado
{
    [Key]
    public int EmpleadoId { get; set; }

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

    public DateOnly? FechaIngreso { get; set; }

    public bool Activo { get; set; } = true;

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Empleado")]
    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    [InverseProperty("Empleado")]
    public virtual ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>();
}
