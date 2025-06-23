using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Factura
{
    [Key]
    public int FacturaId { get; set; }

    public int ClienteId { get; set; }

    public int VehiculoId { get; set; }

    public DateOnly Fecha { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontoTotal { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("Facturas")]
    public virtual Cliente Cliente { get; set; } = null!;

    [InverseProperty("Factura")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    [ForeignKey("VehiculoId")]
    [InverseProperty("Facturas")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}
