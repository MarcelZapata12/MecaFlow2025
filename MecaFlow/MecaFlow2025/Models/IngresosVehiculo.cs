using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class IngresosVehiculo
{
    [Key]
    public int IngresoId { get; set; }

    public int VehiculoId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaIngreso { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }

    [ForeignKey("VehiculoId")]
    [InverseProperty("IngresosVehiculos")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}
