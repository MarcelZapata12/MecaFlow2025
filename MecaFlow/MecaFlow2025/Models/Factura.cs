using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- agrega esto
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Factura
{
    [Key] public int FacturaId { get; set; }

    [Required] public int ClienteId { get; set; }   
    [Required] public int VehiculoId { get; set; }

    public DateOnly Fecha { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    [Range(0, 9999999999.99)]
    public decimal MontoTotal { get; set; }

    // ⬇️ Antes: Observaciones. Ahora: Metodo (mapea a la misma columna)
    [Required(ErrorMessage = "Seleccione un método de pago.")]
    [StringLength(20)]
    [Column("Observaciones")]   // <- reutiliza la columna existente
    public string? Metodo { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("Facturas")]
    [ValidateNever]
    public virtual Cliente Cliente { get; set; } = null!;

    [InverseProperty("Factura")]
    [ValidateNever]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    [ForeignKey("VehiculoId")]
    [InverseProperty("Facturas")]
    [ValidateNever]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}


