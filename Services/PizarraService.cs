using System.Collections.Concurrent;
using Entidades.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using PizarraColaborativa.DTO;

namespace Services
{
    public interface IPizarraService
    {

        Task CrearPizarra(Pizarra pizarra);
        Task<bool>ExisteUsuarioEnPizarra(IdentityUser? usuarioInvitado);
        Task<Pizarra> ObtenerPizarra(Guid id);
        List<PizarraResumenDTO> ObtenerPizarrasDelUsuario(string? idUsuario);
    }
    public class PizarraService : IPizarraService
    {
        private readonly ProyectoPizarraContext _context;

        public PizarraService(ProyectoPizarraContext context)
        {
            _context = context;
        }

 
 
        public Task CrearPizarra(Pizarra pizarra)
        {
               _context.Pizarras.Add(pizarra);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task<bool> ExisteUsuarioEnPizarra(IdentityUser? usuarioInvitado)
        {
            throw new NotImplementedException();
        }

        public async Task<Pizarra> ObtenerPizarra(Guid id)
        {
            var pizarra = await _context.Pizarras.FindAsync(id);
            return pizarra;
        }

        public List<PizarraResumenDTO> ObtenerPizarrasDelUsuario(string? idUsuario)
        {
            return _context.Pizarras.Where(p => p.CreadorId == idUsuario)
                  .Select(p => new PizarraResumenDTO
                  {
                      Id = p.Id,
                      FechaCreacion = p.FechaCreacion,
                      Nombre = p.NombrePizarra
                  }).ToList();
        }
    }



}
