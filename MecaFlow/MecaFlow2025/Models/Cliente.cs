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

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÑáéíóúñ ]+$",
        ErrorMessage = "El nombre solo debe contener letras y espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$",
        ErrorMessage = "El correo debe contener @ y terminar en .com.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20)]
    [RegularExpression(@"^\d{8,12}$",
        ErrorMessage = "El teléfono debe tener solo números (8 a 12 dígitos).")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección/provincia es obligatoria.")]
    [StringLength(200)]
    public string Direccion { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Cliente")]
    public virtual ICollection<CuentasCliente> CuentasClientes { get; set; } = new List<CuentasCliente>();

    [InverseProperty("Cliente")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Cliente")]
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
