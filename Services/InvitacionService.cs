using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IInvitacionService
    {
        void AgregarInvitacion(InvitacionPizarra invitacion);
        Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo);
        Task<string> CrearInvitacionAsync(Guid pizarraId, string usuarioRemitenteId, int rolId);
    }
    public class InvitacionService : IInvitacionService
    {
        private readonly ProyectoPizarraContext _context;
        public InvitacionService(ProyectoPizarraContext context)
        {
            _context = context;
        }
        public void AgregarInvitacion(InvitacionPizarra invitacion)
        {
            _context.InvitacionPizarras.Add(invitacion);
            _context.SaveChanges();
        }

        public Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo)
        {
            return _context.InvitacionPizarras
                .Include(i => i.Rol)
                .FirstOrDefaultAsync(i => i.CodigoInvitacion == codigo);
        }

        public async Task<string> CrearInvitacionAsync(Guid pizarraId, string usuarioRemitenteId, int rolId)
        {
            var rolExiste = await _context.RolEnPizarras.AnyAsync(r => r.Id == rolId);
            if (!rolExiste) throw new ArgumentException("Rol inválido");

            var codigo = Guid.NewGuid().ToString("N");

            var invitacion = new InvitacionPizarra
            {
                PizarraId = pizarraId,
                UsuarioRemitenteId = usuarioRemitenteId,
                CodigoInvitacion = codigo,
                FechaInvitacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
                RolId = rolId
            };

            await _context.InvitacionPizarras.AddAsync(invitacion);
            await _context.SaveChangesAsync();

            return codigo;
        }
    }
}
