using System;

namespace DTO;

public class MensajeDTO
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = null!;
    public string NombreUsuario { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public DateTime FechaPublicacion { get; set; }
    public Guid PizarraId { get; set; }
    public bool VistoPorUsuarioActual { get; set; }
}
