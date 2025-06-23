namespace DTO;

public class MensajeDTO
{
    public int Id { get; set; }
    public string? UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public Guid PizarraId { get; set; }
    public bool VistoPorUsuarioActual { get; set; }
}
