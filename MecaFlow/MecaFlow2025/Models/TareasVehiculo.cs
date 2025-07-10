using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

[Table("TareasVehiculo")]
public partial class TareasVehiculo
{
    [Key]
    public int TareaId { get; set; }

    public int VehiculoId { get; set; }

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Realizada { get; set; }

    [ForeignKey("VehiculoId")]
    [InverseProperty("TareasVehiculos")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}
