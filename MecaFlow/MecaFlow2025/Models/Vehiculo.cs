using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecaFlow2025.Models;

[Index("Placa", Name = "UQ__Vehiculo__8310F99D141C6448", IsUnique = true)]
public partial class Vehiculo
{
    [Key]
    public int VehiculoId { get; set; }

    [StringLength(20)]
    public string Placa { get; set; } = null!;

    [StringLength(50)]
    public string? Marca { get; set; }

    [StringLength(50)]
    public string? Modelo { get; set; }

    public int? Anio { get; set; }

    public int ClienteId { get; set; }


    /*
    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }
    */

    //prueba de fecha
    [Column(TypeName = "datetime")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? FechaRegistro { get; set; }



    [ForeignKey("ClienteId")]
    [InverseProperty("Vehiculos")]

    [BindNever]
    public virtual Cliente Cliente { get; set; } = null!;

    [InverseProperty("Vehiculo")]
    public virtual ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>();

    [InverseProperty("Vehiculo")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Vehiculo")]
    public virtual ICollection<IngresosVehiculo> IngresosVehiculos { get; set; } = new List<IngresosVehiculo>();

    [InverseProperty("Vehiculo")]
    public virtual ICollection<TareasVehiculo> TareasVehiculos { get; set; } = new List<TareasVehiculo>();
}
