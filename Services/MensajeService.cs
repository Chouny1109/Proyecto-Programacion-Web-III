using DTO;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IMensajeService
    {
        Task<List<MensajeDTO>> ObtenerMensajesAsync(Guid pizarraId, string userId);
        Task GuardarMensajeAsync(string userId, Guid pizarraId, string nombreUsuario, string descripcion);
        Task MarcarTodosLosMensajesComoVistosAsync(string userId, Guid pizarraId);
        Task<int> CantidadMensajesNoVistosAsync(string userId, Guid pizarraId);
        Task<List<string>> ObtenerUsuariosDelChatAsync(Guid pizarraId);
    }

    public class MensajeService(ProyectoPizarraContext context) : IMensajeService
    {
        private readonly ProyectoPizarraContext _context = context;

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

        public async Task GuardarMensajeAsync(string userId, Guid pizarraId, string nombreUsuario,
            string descripcion)
        {
            var mensaje = new Mensaje
            {
                UsuarioId = userId,
                PizarraId = pizarraId,
                NombreUsuario = nombreUsuario,
                Descripcion = descripcion,
                FechaPublicacion = DateTime.UtcNow
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();
        }

        public async Task MarcarTodosLosMensajesComoVistosAsync(string userId, Guid pizarraId)
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

        public async Task<int> CantidadMensajesNoVistosAsync(string userId, Guid pizarraId)
        {
            var mensajesNoVistos = await _context.Mensajes
                .Where(m => m.PizarraId == pizarraId && m.UsuarioId != userId)
                .Where(m => !m.MensajeVistos.Any(v => v.UsuarioId == userId))
                .CountAsync();

            return mensajesNoVistos;
        }

        public async Task<List<string>> ObtenerUsuariosDelChatAsync(Guid pizarraId)
        {
            return await _context.PizarraUsuarios
                .Where(pu => pu.PizarraId == pizarraId)
                .Select(pu => pu.UsuarioId)
                .ToListAsync();
        }
    }
}
