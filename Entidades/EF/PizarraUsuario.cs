using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class PizarraUsuario
{
    public Guid PizarraId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public virtual Pizarra Pizarra { get; set; } = null!;

    public virtual AspNetUser Usuario { get; set; } = null!;
}
