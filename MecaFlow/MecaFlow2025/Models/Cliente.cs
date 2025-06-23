using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Cliente
{
    [Key]
    public int ClienteId { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    public string? Correo { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Cliente")]
    public virtual ICollection<CuentasCliente> CuentasClientes { get; set; } = new List<CuentasCliente>();

    [InverseProperty("Cliente")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Cliente")]
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
