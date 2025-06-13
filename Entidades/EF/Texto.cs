using System;
using System.Collections.Generic;

namespace Entidades.EF;

public partial class Texto
{
    public string Id { get; set; }

    public Guid PizarraId { get; set; }

    public string? Contenido { get; set; }

    public double? PosX { get; set; }

    public double? PosY { get; set; }

    public string? Color { get; set; }

    public int? Tamano { get; set; }


    public virtual Pizarra Pizarra { get; set; } = null!;
}
