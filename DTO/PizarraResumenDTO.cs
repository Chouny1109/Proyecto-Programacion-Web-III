namespace DTO;

public class PizarraResumenDTO
{
    public Guid Id { get; set; }
    public string? Nombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int Rol { get; set; } 
    public List<string> Integrantes { get; set; } = new();
}
