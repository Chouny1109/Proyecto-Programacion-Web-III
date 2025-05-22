using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class Trazo
{
    public int Id { get; set; }

    public Guid PizarraId { get; set; }

    public string? Color { get; set; }

    public double? Xinicio { get; set; }

    public double? Yinicio { get; set; }

    public double? Xfin { get; set; }

    public double? Yfin { get; set; }

    public int? Grosor { get; set; }

    public virtual Pizarra Pizarra { get; set; } = null!;
}
