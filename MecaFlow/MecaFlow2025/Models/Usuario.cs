using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

[Index("Username", Name = "UQ__Usuarios__536C85E49978C5EA", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int UsuarioId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100)]
    public string? Correo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Usuarios")]
    public virtual ICollection<Role> Rols { get; set; } = new List<Role>();
}
