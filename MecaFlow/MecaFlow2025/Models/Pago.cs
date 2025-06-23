using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Pago
{
    [Key]
    public int PagoId { get; set; }

    public int FacturaId { get; set; }

    public DateOnly FechaPago { get; set; }

    [StringLength(50)]
    public string? MetodoPago { get; set; }

    [ForeignKey("FacturaId")]
    [InverseProperty("Pagos")]
    public virtual Factura Factura { get; set; } = null!;
}
