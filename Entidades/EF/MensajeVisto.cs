using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Entidades.EF;

public partial class MensajeVisto
{
    public int MensajeId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public DateTime FechaVisto { get; set; }

    public virtual Mensaje Mensaje { get; set; } = null!;

    public virtual IdentityUser Usuario { get; set; } = null!;
}
