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

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(20)]
    public string? Cedula { get; set; }

    [StringLength(100)]
    public string? Correo { get; set; }

    [StringLength(50)]
    public string? Puesto { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public bool Activo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Empleado")]
    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    [InverseProperty("Empleado")]
    public virtual ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>();
}
