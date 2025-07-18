using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecaFlow2025.Models
{
    public class Marca
    {
        [Key]
        public int MarcaId { get; set; }
        [Required]
        [StringLength(50)]
        public string? Nombre { get; set; }

        [InverseProperty("Marca")]
        public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }

}
