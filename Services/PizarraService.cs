using DTO;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IPizarraService
    {
        Task<List<PizarraResumenDTO>> ObtenerPizarrasFiltradasAsync(string? userId, string? filtroRol, string busqueda);
        Task<Pizarra?> ObtenerPizarra(Guid id);
        Task<int> ObtenerRolIdAsync(string rolNombre);
        Task<bool> EsAdminDeLaPizarraAsync(string userId, Guid pizarraId);
        Task<bool> EsLector(string? userIdentifier, string pizarraId);
        Task<string?> ObtenerRolDeUsuarioEnPizarraAsync(string userId, Guid pizarraId);
        Task<List<UserNameIDPizarraDTO>> ObtenerUsuariosDePizarra(Guid pizarraId);
        Task<Guid> CrearPizarraAsync(string userId, string pizarraNombre);
        Task EliminarPizarraAsync(Guid pizarraId);
        Task ActualizarPizarra(Guid id, string nuevoNombre);
        void AgregarTrazo(Trazo trazo);
        Task<List<Trazo>> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid);
        Task PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary);
        Task BorrarTrazosExistentesPizarra(List<Trazo> existentes);
        Task<Texto?> ObtenerTextoPorId(string idTexto, string pizarraId);
        Task<List<Texto>> ObtenerTextosDeUnaPizarra(Guid pizarraguid);
        Task PersistirTextosBD(Dictionary<string, List<Texto>> dictionary);
        Task BorrarTextosExistentesPizarra(List<Texto> existentes);
        Task<string?> ObtenerColorFondoDeUnaPizarra(Guid guid);
        Task CambiarColorFondoPizarra(string pizarraId, string colorFondo);
        Task SetColorBlancoPizarra(Guid pizarraGUID);
        Task EliminarUsuarioDePizarra(string userId, Guid pizarraId);
    }

    public class PizarraService(ProyectoPizarraContext context) : IPizarraService
    {
        private readonly ProyectoPizarraContext _context = context;

        private async Task<List<PizarraResumenDTO>> ObtenerPizarrasAsync(string? userId)
        {
            var pizarrasUsuario = await _context.PizarraUsuarios
                .Where(pu => pu.UsuarioId == userId)
                .Include(pu => pu.Pizarra)
                .Include(pu => pu.Rol)
                .OrderByDescending(pu => pu.Pizarra.FechaCreacion)
                .ToListAsync();

            var dto = new List<PizarraResumenDTO>();

            foreach (var pu in pizarrasUsuario)
            {
                var integrantes = await _context.PizarraUsuarios
                    .Where(x => x.PizarraId == pu.PizarraId && x.UsuarioId != userId)
                    .Select(x => x.UsuarioId)
                    .ToListAsync();

                dto.Add(new PizarraResumenDTO
                {
                    Id = pu.Pizarra.Id,
                    FechaCreacion = pu.Pizarra.FechaCreacion,
                    Nombre = pu.Pizarra.NombrePizarra,
                    Rol = pu.Rol.Id,
                    Integrantes = integrantes
                });
            }

            return dto;
        }

        public async Task<List<PizarraResumenDTO>> ObtenerPizarrasFiltradasAsync(string? userId,
            string? filtroRol, string busqueda)
        {
            var pizarras = await ObtenerPizarrasAsync(userId);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                pizarras = pizarras
                    .Where(p => p.Nombre != null && p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filtroRol))
            {
                int rolIdAdmin = await ObtenerRolIdAsync("Admin");

                if (filtroRol == "admin")
                {
                    pizarras = pizarras.Where(p => p.Rol == rolIdAdmin).ToList();
                }
                else if (filtroRol == "noAdmin")
                {
                    pizarras = pizarras.Where(p => p.Rol != rolIdAdmin).ToList();
                }
            }

            return pizarras;
        }

        public async Task<Pizarra?> ObtenerPizarra(Guid id)
        {
            return await _context.Pizarras.FindAsync(id);
        }

        public async Task<int> ObtenerRolIdAsync(string rolNombre)
        {
            return await _context.RolEnPizarras
                .Where(r => r.Nombre == rolNombre)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EsAdminDeLaPizarraAsync(string userId, Guid pizarraId)
        {
            int rolIdAdmin = await ObtenerRolIdAsync("Admin");

            return await _context.PizarraUsuarios
                .AnyAsync(pu => pu.UsuarioId == userId && pu.PizarraId == pizarraId && pu.RolId == rolIdAdmin);
        }

        public Task<bool> EsLector(string? userIdentifier, string pizarraId)
        {
            return _context.PizarraUsuarios
                .AnyAsync(pu => pu.UsuarioId == userIdentifier && pu.PizarraId == Guid.Parse(pizarraId) && pu.Rol.Nombre == "Lector");
        }

        public async Task<string?> ObtenerRolDeUsuarioEnPizarraAsync(string userId, Guid pizarraId)
        {
            return await _context.PizarraUsuarios
                .Where(pu => pu.UsuarioId == userId && pu.PizarraId == pizarraId)
                .Select(pu => pu.Rol.Nombre)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserNameIDPizarraDTO>> ObtenerUsuariosDePizarra(Guid pizarraId)
        {
            return await _context.PizarraUsuarios
                .Where(up => up.PizarraId == pizarraId && up.RolId != 3)
                .Select(up => new UserNameIDPizarraDTO
                {
                    UserId = up.UsuarioId,
                    UserName = _context.Users
                        .Where(u => u.Id == up.UsuarioId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<Guid> CrearPizarraAsync(string userId, string pizarraNombre)
        {
            var pizarra = new Pizarra
            {
                Id = Guid.NewGuid(),
                CreadorId = userId,
                NombrePizarra = pizarraNombre,
                FechaCreacion = DateTime.UtcNow
            };

            var rolAdminId = await ObtenerRolIdAsync("Admin");

            var usuarioAdmin = new PizarraUsuario
            {
                PizarraId = pizarra.Id,
                UsuarioId = userId,
                RolId = rolAdminId
            };

            await _context.Pizarras.AddAsync(pizarra);
            await _context.PizarraUsuarios.AddAsync(usuarioAdmin);
            await _context.SaveChangesAsync();

            return pizarra.Id;
        }

        public async Task EliminarPizarraAsync(Guid pizarraId)
        {
            var pizarra = await _context.Pizarras.FindAsync(pizarraId);
            if (pizarra == null) return;

            _context.Pizarras.Remove(pizarra);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarPizarra(Guid id, string nuevoNombre)
        {
            var pizarra = ObtenerPizarra(id).Result;

            if (pizarra != null)
            {
                pizarra.NombrePizarra = nuevoNombre;
                await _context.SaveChangesAsync();
            }
        }

        public void AgregarTrazo(Trazo trazo)
        {
            _context.Trazos.Add(trazo);
            _context.SaveChanges();
        }

        public async Task<List<Trazo>> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid)
        {
            return await _context.Trazos.Where(t => t.PizarraId == pizarraGuid).ToListAsync();
        }

        public async Task PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary)
        {
            foreach (var (pizarraId, trazos) in dictionary)
            {
                var pizarraguid = Guid.Parse(pizarraId);
                var existentes = await ObtenerTrazosDeUnaPizarra(pizarraguid);
                await BorrarTrazosExistentesPizarra(existentes);

                foreach (var trazo in trazos)
                {
                    trazo.Id = 0;
                    trazo.PizarraId = pizarraguid;
                    AgregarTrazo(trazo);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task BorrarTrazosExistentesPizarra(List<Trazo> existentes)
        {
            _context.Trazos.RemoveRange(existentes);
            await _context.SaveChangesAsync();
        }

        public async Task<Texto?> ObtenerTextoPorId(string idTexto, string pizarraId)
        {
            return await _context.Textos.FirstOrDefaultAsync(t => t.Id.Equals(idTexto)
                && t.PizarraId.Equals(pizarraId));
        }

        public async Task<List<Texto>> ObtenerTextosDeUnaPizarra(Guid pizarraguid)
        {
            return await _context.Textos.Where(t => t.PizarraId == pizarraguid).ToListAsync();
        }

        public async Task PersistirTextosBD(Dictionary<string, List<Texto>> dictionary)
        {
            foreach (var (pizarraId, textosMemoria) in dictionary)
            {
                var pizarraguid = Guid.Parse(pizarraId);

                // 1. Traer textos actuales de la base
                var textosBD = await _context.Textos
                    .Where(t => t.PizarraId == pizarraguid)
                    .ToListAsync();

                // 2. Identificar los textos que fueron eliminados de memoria
                var textosAEliminar = textosBD
                    .Where(tbd => !textosMemoria.Any(tm => tm.Id == tbd.Id))
                    .ToList();

                // 3. Eliminar esos textos de la base
                if (textosAEliminar.Any())
                {
                    _context.Textos.RemoveRange(textosAEliminar);
                }

                // 4. Actualizar existentes y agregar nuevos
                foreach (var texto in textosMemoria)
                {
                    texto.PizarraId = pizarraguid;
                    var textoExistente = textosBD.FirstOrDefault(t => t.Id == texto.Id);
                    if (textoExistente != null)
                    {
                        textoExistente.Contenido = texto.Contenido;
                        textoExistente.PosX = texto.PosX;
                        textoExistente.PosY = texto.PosY;
                        textoExistente.Color = texto.Color;
                        textoExistente.Tamano = texto.Tamano;
                    }
                    else
                    {
                        if (!_context.Textos.Local.Any(t => t.Id == texto.Id))
                        {
                            _context.Textos.Add(texto);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task BorrarTextosExistentesPizarra(List<Texto> existentes)
        {
            _context.Textos.RemoveRange(existentes);
            await _context.SaveChangesAsync();
        }

        public async Task<string?> ObtenerColorFondoDeUnaPizarra(Guid guid)
        {
            var pizarra = await ObtenerPizarra(guid);
            return pizarra?.ColorFondo;
        }

        public async Task CambiarColorFondoPizarra(string pizarraId, string colorFondo)
        {
            var pizarra = await ObtenerPizarra(Guid.Parse(pizarraId));

            if (pizarra != null)
            {
                pizarra.ColorFondo = colorFondo;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetColorBlancoPizarra(Guid pizarraGUID)
        {
            var pizarra = await ObtenerPizarra(pizarraGUID);

            if (pizarra != null)
            {
                pizarra.ColorFondo = "#ffffff";
                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarUsuarioDePizarra(string userId, Guid pizarraId)
        {
            var relacion = await _context.PizarraUsuarios
                .FirstOrDefaultAsync(up => up.UsuarioId == userId && up.PizarraId == pizarraId);

            if (relacion != null)
            {
                _context.PizarraUsuarios.Remove(relacion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
