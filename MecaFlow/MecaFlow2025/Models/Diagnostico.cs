using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Diagnostico
{
    [Key]
    public int DiagnosticoId { get; set; }

    public int VehiculoId { get; set; }

    public DateOnly Fecha { get; set; }

    [StringLength(500)]
    public string? Detalle { get; set; }

    public int? EmpleadoId { get; set; }

    [ForeignKey("EmpleadoId")]
    [InverseProperty("Diagnosticos")]
    public virtual Empleado? Empleado { get; set; }

    [ForeignKey("VehiculoId")]
    [InverseProperty("Diagnosticos")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}
