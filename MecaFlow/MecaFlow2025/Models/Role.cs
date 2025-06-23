using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Role
{
    [Key]
    public int RolId { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [ForeignKey("RolId")]
    [InverseProperty("Rols")]
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();

    [ForeignKey("RolId")]
    [InverseProperty("Rols")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
