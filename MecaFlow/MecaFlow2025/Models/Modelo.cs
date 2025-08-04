namespace MecaFlow2025.Models
{
    public class Modelo
    {
        public int ModeloId { get; set; }
        public string Nombre { get; set; } = null!;

        public int MarcaId { get; set; }
        public Marca Marca { get; set; } = null!;
    }
}
