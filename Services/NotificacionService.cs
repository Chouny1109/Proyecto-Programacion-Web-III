using DTO;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface INotificacionService
    {
        Task<List<NotificacionDTO>> ObtenerHistorialAsync(string userId);
        Task<NotificacionDTO> EnviarNotificacionAsync(string remitenteId, string destinatarioId,
            string titulo, string descripcion, Guid? pizarraId = null, string? pizarraNombre = null,
            int? rol = null);
        Task EliminarNotificacionAsync(Guid notificacionId, string userId);
        Task VaciarBandejaAsync(string userId);
        Task MarcarTodasComoVistasAsync(string userId);
        Task<int> ContarNotificacionesNoVistasAsync(string userId);
    }

    public class NotificacionService(ProyectoPizarraContext context) : INotificacionService
    {
        private readonly ProyectoPizarraContext _context = context;

        public async Task<List<NotificacionDTO>> ObtenerHistorialAsync(string userId)
        {
            var remitente = _context.Users.Find(userId);
            if (remitente == null) return new List<NotificacionDTO>();

            var historial = await _context.NotificacionUsuarios
                .Where(nu => nu.UsuarioId == userId)
                .Include(nu => nu.Notificacion)
                    .ThenInclude(n => n.Pizarra)
                .Include(nu => nu.Usuario)
                .OrderByDescending(nu => nu.Notificacion.FechaCreacion)
                .Select(nu => new NotificacionDTO
                {
                    Id = nu.Notificacion.Id,
                    Titulo = nu.Notificacion.Titulo,
                    Descripcion = nu.Notificacion.Descripcion,
                    FechaCreacion = nu.Notificacion.FechaCreacion,
                    FueVista = nu.FueVista,
                    DestinatarioId = nu.Usuario.Id,
                    DestinatarioNombre = nu.Usuario.UserName ?? null,
                    RemitenteId = remitente.Id,
                    RemitenteNombre = remitente.UserName,
                    PizarraId = nu.Notificacion.PizarraId,
                    PizarraNombre = nu.Notificacion.Pizarra != null
                        ? nu.Notificacion.Pizarra.NombrePizarra : null,
                    Rol = nu.Notificacion.RolEnPizarraId
                })
                .ToListAsync();

            return historial;
        }

        public async Task<NotificacionDTO> EnviarNotificacionAsync(string remitenteId, string destinatarioId,
            string titulo, string descripcion, Guid? pizarraId = null, string? pizarraNombre = null,
            int? rol = null)
        {
            var remitente = await _context.Users.FindAsync(remitenteId);
            var destinatario = await _context.Users.FindAsync(destinatarioId);

            var notificacion = new Notificacion
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Descripcion = descripcion,
                FechaCreacion = DateTime.UtcNow,
                RemitenteId = remitenteId,
                PizarraId = pizarraId,
                RolEnPizarraId = rol
            };

            _context.Notificacions.Add(notificacion);

            var notificacionUsuario = new NotificacionUsuario
            {
                Id = Guid.NewGuid(),
                NotificacionId = notificacion.Id,
                UsuarioId = destinatarioId,
                FueVista = false
            };

            _context.NotificacionUsuarios.Add(notificacionUsuario);
            await _context.SaveChangesAsync();

            var dto = new NotificacionDTO
            {
                Id = notificacion.Id,
                Titulo = notificacion.Titulo,
                Descripcion = notificacion.Descripcion,
                FechaCreacion = notificacion.FechaCreacion,
                FueVista = false,
                DestinatarioId = destinatarioId,
                DestinatarioNombre = destinatario?.UserName,
                RemitenteId = remitenteId,
                RemitenteNombre = remitente?.UserName,
                PizarraId = pizarraId,
                PizarraNombre = pizarraNombre,
                Rol = rol,
            };

            return dto;
        }

        public async Task EliminarNotificacionAsync(Guid notificacionId, string userId)
        {
            var notificacionUsuario = await _context.NotificacionUsuarios
                .FirstOrDefaultAsync(nu => nu.NotificacionId == notificacionId && nu.UsuarioId == userId);

            if (notificacionUsuario != null)
            {
                _context.NotificacionUsuarios.Remove(notificacionUsuario);
                await _context.SaveChangesAsync();
            }
        }

        public async Task VaciarBandejaAsync(string userId)
        {
            var notificacionesUsuario = _context.NotificacionUsuarios.Where(nu => nu.UsuarioId == userId);
            _context.NotificacionUsuarios.RemoveRange(notificacionesUsuario);
            await _context.SaveChangesAsync();
        }

        public async Task MarcarTodasComoVistasAsync(string userId)
        {
            var notificaciones = await _context.NotificacionUsuarios
                .Where(nu => nu.UsuarioId == userId && !nu.FueVista)
                .ToListAsync();

            foreach (var nu in notificaciones)
            {
                nu.FueVista = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarNotificacionesNoVistasAsync(string userId)
        {
            return await _context.NotificacionUsuarios.CountAsync(nu => nu.UsuarioId == userId && !nu.FueVista);
        }
    }
}
