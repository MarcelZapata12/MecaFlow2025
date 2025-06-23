using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Permiso
{
    [Key]
    public int PermisoId { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [ForeignKey("PermisoId")]
    [InverseProperty("Permisos")]
    public virtual ICollection<Role> Rols { get; set; } = new List<Role>();
}
