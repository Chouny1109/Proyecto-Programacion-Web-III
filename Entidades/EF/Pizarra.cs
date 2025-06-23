using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class Pizarra
{
    public Guid Id { get; set; }

    public string CreadorId { get; set; } = null!;

    public string? NombrePizarra { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? ColorFondo { get; set; }

    public virtual ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<PizarraUsuario> PizarraUsuarios { get; set; } = new List<PizarraUsuario>();

    public virtual ICollection<Texto> Textos { get; set; } = new List<Texto>();

    public virtual ICollection<Trazo> Trazos { get; set; } = new List<Trazo>();
}
