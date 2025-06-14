using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class Notificacion
{
    public Guid Id { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<NotificacionUsuario> NotificacionUsuarios { get; set; } = new List<NotificacionUsuario>();
}
