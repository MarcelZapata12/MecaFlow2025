using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class ReportesFinanciero
{
    [Key]
    public int ReporteId { get; set; }

    public DateOnly Fecha { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalIngresos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalGastos { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
