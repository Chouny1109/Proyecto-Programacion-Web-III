namespace DTO;

public class NotificacionDTO
{
    public Guid Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool FueVista { get; set; }
    public string? DestinatarioId { get; set; }
    public string? DestinatarioNombre { get; set; }
    public string? RemitenteId { get; set; }
    public string? RemitenteNombre { get; set; }
    public Guid? PizarraId { get; set; }
    public string? PizarraNombre { get; set; }
    public int? Rol { get; set; }
}
