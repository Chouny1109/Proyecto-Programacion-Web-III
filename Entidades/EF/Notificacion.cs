using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Entidades.EF;

public partial class Notificacion
{
    public Guid Id { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string RemitenteId { get; set; } = null!;

    public Guid? PizarraId { get; set; }

    public int? RolEnPizarraId { get; set; }

    public virtual ICollection<NotificacionUsuario> NotificacionUsuarios { get; set; } = new List<NotificacionUsuario>();

    public virtual Pizarra? Pizarra { get; set; }

    public virtual IdentityUser Remitente { get; set; } = null!;

    public virtual RolEnPizarra? RolEnPizarra { get; set; }
}
