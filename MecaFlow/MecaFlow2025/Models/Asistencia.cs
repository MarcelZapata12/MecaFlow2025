using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class Asistencia
{
    [Key]
    public int AsistenciaId { get; set; }

    public int EmpleadoId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? HoraEntrada { get; set; }

    public TimeOnly? HoraSalida { get; set; }

    [ForeignKey("EmpleadoId")]
    [InverseProperty("Asistencia")]
    public virtual Empleado Empleado { get; set; } = null!;
}
