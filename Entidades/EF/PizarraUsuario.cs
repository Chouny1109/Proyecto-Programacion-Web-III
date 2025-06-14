using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Entidades.EF;

public partial class PizarraUsuario
{
    public Guid PizarraId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public int RolId { get; set; }

    public virtual Pizarra Pizarra { get; set; } = null!;

    public virtual RolEnPizarra Rol { get; set; } = null!;

    public virtual IdentityUser Usuario { get; set; } = null!;
}
