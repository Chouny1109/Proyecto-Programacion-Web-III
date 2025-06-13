using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Entidades.EF;

public partial class Mensaje
{
    public int Id { get; set; }

    public Guid PizarraId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public string NombreUsuario { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; }

    public virtual ICollection<MensajeVisto> MensajeVistos { get; set; } = new List<MensajeVisto>();

    public virtual Pizarra Pizarra { get; set; } = null!;

    public virtual IdentityUser Usuario { get; set; } = null!;
}
