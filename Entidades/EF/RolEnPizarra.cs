using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class RolEnPizarra
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<InvitacionPizarra> InvitacionPizarras { get; set; } = new List<InvitacionPizarra>();

    public virtual ICollection<PizarraUsuario> PizarraUsuarios { get; set; } = new List<PizarraUsuario>();
}
