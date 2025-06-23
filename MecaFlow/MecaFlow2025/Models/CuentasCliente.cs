using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

[Index("Username", Name = "UQ__CuentasC__536C85E49E4F4CF1", IsUnique = true)]
public partial class CuentasCliente
{
    [Key]
    public int CuentaClienteId { get; set; }

    public int ClienteId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("CuentasClientes")]
    public virtual Cliente Cliente { get; set; } = null!;
}
