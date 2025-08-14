using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecaFlow2025.Models;

[Table("TareasVehiculo")]
public partial class TareasVehiculo
{
    [Key]
    public int TareaId { get; set; }

    public int VehiculoId { get; set; }

    [StringLength(30)]
    public string? Sector { get; set; }

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? FechaRegistro { get; set; }

    public bool Realizada { get; set; }

    [ForeignKey("VehiculoId")]
    [InverseProperty("TareasVehiculos")]
    [ValidateNever]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}