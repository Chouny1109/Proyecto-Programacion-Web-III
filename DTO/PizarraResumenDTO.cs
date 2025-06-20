﻿using DTO;

namespace PizarraColaborativa.DTO
{
    public class PizarraResumenDTO
    {
        public Guid Id{ get; set; }
        public string? Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Rol { get; set; } = string.Empty;
    }
}
