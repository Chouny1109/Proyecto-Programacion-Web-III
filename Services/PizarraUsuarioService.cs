using DTO;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IPizarraUsuarioService
    {
        Task<bool> ExisteRelacionPizarraUsuarioAsync(string userId, Guid pizarraId);
        Task CrearRelacionPizarraUsuarioAsync(string userId, Guid pizarraId, int rolId);
    }

    public class PizarraUsuarioService(ProyectoPizarraContext context) : IPizarraUsuarioService
    {
        private readonly ProyectoPizarraContext _context = context;

        public async Task<bool> ExisteRelacionPizarraUsuarioAsync(string userId, Guid pizarraId)
        {
            return await _context.PizarraUsuarios.AnyAsync(pu => pu.PizarraId == pizarraId && pu.UsuarioId == userId);
        }

        public async Task CrearRelacionPizarraUsuarioAsync(string userId, Guid pizarraId, int rolId)
        {
            var existe = await _context.PizarraUsuarios
                .FirstOrDefaultAsync(pu => pu.PizarraId == pizarraId && pu.UsuarioId == userId);

            if (existe is null)
            {
                var pizarraUsuario = new PizarraUsuario
                {
                    PizarraId = pizarraId,
                    UsuarioId = userId,
                    RolId = rolId
                };
                await _context.PizarraUsuarios.AddAsync(pizarraUsuario);
            }
            else
            {
                existe.RolId = rolId;
                _context.PizarraUsuarios.Update(existe);
            }

            await _context.SaveChangesAsync();
        }
    }
}
