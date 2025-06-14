using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Entidades.EF;

public partial class NotificacionUsuario
{
    public Guid Id { get; set; }

    public Guid NotificacionId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public bool FueVista { get; set; }

    public virtual Notificacion Notificacion { get; set; } = null!;

    public virtual IdentityUser Usuario { get; set; } = null!;
}
