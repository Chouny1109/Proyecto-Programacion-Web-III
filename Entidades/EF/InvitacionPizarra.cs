using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class InvitacionPizarra
{
    public int Id { get; set; }

    public Guid PizarraId { get; set; }

    public string CodigoInvitacion { get; set; } = null!;

    public string UsuarioRemitenteId { get; set; } = null!;

    public DateTime FechaInvitacion { get; set; }

    public DateTime? FechaExpiracion { get; set; }

    public int Rol { get; set; }

    public virtual Pizarra Pizarra { get; set; } = null!;

    public virtual AspNetUser UsuarioRemitente { get; set; } = null!;
}
