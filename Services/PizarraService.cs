using System.Collections.Concurrent;
using System.Security.Cryptography;
using DTO;
using Entidades.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using PizarraColaborativa.DTO;

namespace Services
{

    public interface IPizarraService
    {
        Task ActualizarPizarra(Guid pizarraIdGUID, string nuevoNombre);
        void AgregarTrazo(Trazo trazo);
        Task BorrarTrazosExistentesPizarra(List<Trazo> existentes);
        Task BorrarTextosExistentesPizarra(List<Texto> existentes);
        Task<bool> ExisteUsuarioEnPizarra(string id, Guid pizarraId);
        Task<Pizarra> ObtenerPizarra(Guid id);
        Task<Texto> ObtenerTextoPorId(string idTexto, string pizarraId);
        Task<List<Texto>> ObtenerTextosDeUnaPizarra(Guid pizarraGuid);
        Task<List<Trazo>> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid);
        Task PersistirTextosBD(Dictionary<string, List<Texto>> dictionary);
        Task PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary);
        Task<string?> ObtenerColorFondoDeUnaPizarra(Guid guid);
        Task CambiarColorFondoPizarra(string pizarraId, string colorFondo);
        Task CrearPizarraAsync(Pizarra pizarra, string creadorId);
        Task AgregarOActualizarUsuarioEnPizarraAsync(PizarraUsuario pizarraUsuario);
        Task<bool> EsAdminDeLaPizarraAsync(string userId, Guid pizarraId);
        Task<List<PizarraResumenDTO>> ObtenerPizarrasDelUsuarioAsync(string userId);
        Task<List<PizarraResumenDTO>> ObtenerPizarrasFiltradasAsync(string userId, int? filtroRol, string busqueda);
        Task<bool> EliminarPizarraAsync(Guid pizarraId);
        Task<List<MensajeDTO>> ObtenerMensajesAsync(Guid pizarraId, string userId);
        Task<MensajeDTO> GuardarMensajeAsync(MensajeDTO mensaje);
        Task MarcarTodosLosMensajesComoVistosAsync(Guid pizarraId, string userId);
        Task<int> ObtenerCantidadMensajesNoVistosAsync(string userId, Guid pizarraId);
        Task SetColorBlancoPizarra(Guid pizarraGUID);
    }

    public class PizarraService : IPizarraService
    {
        private readonly ProyectoPizarraContext _context;

        public PizarraService(ProyectoPizarraContext context)
        {
            _context = context;
        }

        public void AgregarTrazo(Trazo trazo)
        {
            _context.Trazos.Add(trazo);
            _context.SaveChanges();

        }

        public async Task BorrarTrazosExistentesPizarra(List<Trazo> existentes)
        {
            _context.Trazos.RemoveRange(existentes);
            await _context.SaveChangesAsync();
        }       

        public async Task<bool> ExisteUsuarioEnPizarra(string id, Guid pizarraId)
        {
            return await _context.PizarraUsuarios.AnyAsync(pu => pu.PizarraId == pizarraId && pu.UsuarioId == id);
        }

        public async Task<Pizarra> ObtenerPizarra(Guid id)
        {
            var pizarra = await _context.Pizarras.FindAsync(id);
            return pizarra;
        }

        public async Task<Texto> ObtenerTextoPorId(string idTexto, string pizarraId)
        {
            return await _context.Textos.FirstOrDefaultAsync(t => t.Id.Equals(idTexto)
             && t.PizarraId.Equals(pizarraId));

        }

        public async Task PersistirTextosBD(Dictionary<string, List<Texto>> dictionary)
        {
            foreach (var (pizarraId, textos) in dictionary)
            {
                var pizarraguid = Guid.Parse(pizarraId);
                var existentes = await ObtenerTextosDeUnaPizarra(pizarraguid);

                foreach (var texto in textos)
                {
                    texto.PizarraId = pizarraguid;
                    var textoExistente = existentes.FirstOrDefault(t => t.Id == texto.Id);
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

        private void AgregarTexto(Texto texto)
        {
            _context.Textos.Add(texto);
            _context.SaveChanges();
        }

        public async Task<List<Texto>> ObtenerTextosDeUnaPizarra(Guid pizarraguid)
        {
            return await _context.Textos.Where(t => t.PizarraId == pizarraguid).ToListAsync();
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

        public async Task<List<Trazo>> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid)
        {
            return await _context.Trazos.Where(t => t.PizarraId == pizarraGuid).ToListAsync();
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

        public async Task<string?> ObtenerColorFondoDeUnaPizarra(Guid guid)
        {
            var pizarra = await ObtenerPizarra(guid);

            return pizarra.ColorFondo;
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

        public async Task CrearPizarraAsync(Pizarra pizarra, string creadorId)
        {
            var usuarioAdmin = new PizarraUsuario
            {
                PizarraId = pizarra.Id,
                UsuarioId = creadorId,
                Rol = RolEnPizarra.Admin
            };

            await _context.Pizarras.AddAsync(pizarra);
            await _context.PizarraUsuarios.AddAsync(usuarioAdmin);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarOActualizarUsuarioEnPizarraAsync(PizarraUsuario usuarioPizarra)
        {
            var existente = await _context.PizarraUsuarios
                .FirstOrDefaultAsync(pu => pu.PizarraId == usuarioPizarra.PizarraId && pu.UsuarioId == usuarioPizarra.UsuarioId);

            if (existente is null)
            {
                await _context.PizarraUsuarios.AddAsync(usuarioPizarra);
            }
            else
            {
                if (existente.Rol == RolEnPizarra.Admin)
                {
                    throw new InvalidOperationException("No se puede modificar el rol del administrador de la pizarra.");
                }

                existente.Rol = usuarioPizarra.Rol;
                _context.PizarraUsuarios.Update(existente);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> EsAdminDeLaPizarraAsync(string userId, Guid pizarraId)
        {
            return await _context.PizarraUsuarios
                .AnyAsync(pu => pu.UsuarioId == userId &&
                    pu.PizarraId == pizarraId &&
                    pu.Rol == RolEnPizarra.Admin);
        }

        public async Task<List<PizarraResumenDTO>> ObtenerPizarrasDelUsuarioAsync(string userId)
        {
            return await _context.PizarraUsuarios
                .Where(pu => pu.UsuarioId == userId)
                .Include(pu => pu.Pizarra)
                .OrderByDescending(pu => pu.Pizarra.FechaCreacion)
                .Select(pu => new PizarraResumenDTO
                {
                    Id = pu.Pizarra.Id,
                    FechaCreacion = pu.Pizarra.FechaCreacion,
                    Nombre = pu.Pizarra.NombrePizarra,
                    Rol = Enum.Parse<RolEnPizarra>(pu.Rol.ToString())
                })
                .ToListAsync();
        }

        public async Task<List<PizarraResumenDTO>> ObtenerPizarrasFiltradasAsync(string userId, int? filtroRol, string busqueda)
        {
            var pizarras = await ObtenerPizarrasDelUsuarioAsync(userId);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                pizarras = pizarras
                    .Where(p => p.Nombre != null && p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (filtroRol.HasValue)
            {
                var rolFiltrado = (RolEnPizarra)filtroRol;

                if (rolFiltrado == RolEnPizarra.Admin)
                {
                    pizarras = pizarras.Where(p => p.Rol == RolEnPizarra.Admin).ToList();
                }
                else
                {
                    pizarras = pizarras.Where(p => p.Rol != RolEnPizarra.Admin).ToList();
                }
            }

            return pizarras;
        }

        public async Task<bool> EliminarPizarraAsync(Guid pizarraId)
        {
            var pizarra = await _context.Pizarras.FindAsync(pizarraId);
            if (pizarra == null) return false;

            var invitaciones = await _context.InvitacionPizarras
                .Where(i => i.PizarraId == pizarraId)
                .ToListAsync();

            if (invitaciones.Count > 0)
                _context.InvitacionPizarras.RemoveRange(invitaciones);

            _context.Pizarras.Remove(pizarra);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MensajeDTO>> ObtenerMensajesAsync(Guid pizarraId, string userId)
        {
            var mensajes = await _context.Mensajes
                .Where(m => m.PizarraId == pizarraId)
                .OrderBy(m => m.FechaPublicacion)
                .Select(m => new MensajeDTO
                {
                    Id = m.Id,
                    PizarraId = m.PizarraId,
                    UsuarioId = m.UsuarioId,
                    NombreUsuario = m.NombreUsuario,
                    Descripcion = m.Descripcion,
                    FechaPublicacion = m.FechaPublicacion,
                    VistoPorUsuarioActual = m.MensajeVistos.Any(v => v.UsuarioId == userId)
                })
                .ToListAsync();

            return mensajes;
        }

        public async Task<MensajeDTO> GuardarMensajeAsync(MensajeDTO dto)
        {
            var mensaje = new Mensaje
            {
                PizarraId = dto.PizarraId,
                UsuarioId = dto.UsuarioId,
                NombreUsuario = dto.NombreUsuario,
                Descripcion = dto.Descripcion,
                FechaPublicacion = DateTime.UtcNow
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            dto.Id = mensaje.Id;
            dto.FechaPublicacion = mensaje.FechaPublicacion;
            return dto;
        }

        public async Task MarcarTodosLosMensajesComoVistosAsync(Guid pizarraId, string userId)
        {
            var mensajes = await _context.Mensajes
                .Where(m => m.PizarraId == pizarraId && m.UsuarioId != userId)
                .Where(m => !_context.MensajeVistos.Any(v => v.MensajeId == m.Id && v.UsuarioId == userId))
                .ToListAsync();

            foreach (var mensaje in mensajes)
            {
                _context.MensajeVistos.Add(new MensajeVisto
                    {
                        MensajeId = mensaje.Id,
                        UsuarioId = userId,
                        FechaVisto = DateTime.UtcNow
                    });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ObtenerCantidadMensajesNoVistosAsync(string userId, Guid pizarraId)
        {
            var mensajesNoVistos = await _context.Mensajes
                .Where(m => m.PizarraId == pizarraId && m.UsuarioId != userId)
                .Where(m => !m.MensajeVistos.Any(v => v.UsuarioId == userId))
                .CountAsync();

            return mensajesNoVistos;
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
    }
}