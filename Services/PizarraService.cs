using System.Collections.Concurrent;
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
       Task AgregarUsuarioALaPizarra(PizarraUsuario pizarraUsuario);
        Task CrearPizarra(Pizarra pizarra);
        Task<bool> EsAdminDeLaPizarra(string idUsuario, Guid id);
        Task<bool> ExisteUsuarioEnPizarra(string id, Guid pizarraId);
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

        public Task AgregarUsuarioALaPizarra(PizarraUsuario pizarraUsuario)
        {
            _context.PizarraUsuarios.AddAsync(pizarraUsuario);
            _context.SaveChangesAsync();
            return Task.CompletedTask;
        }

        public Task CrearPizarra(Pizarra pizarra)
        {
               _context.Pizarras.Add(pizarra);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public async Task<bool> EsAdminDeLaPizarra(string idUsuario, Guid id)
        {
           return await _context.PizarraUsuarios.AnyAsync(pu => pu.PizarraId== id && pu.UsuarioId== idUsuario 
           && pu.Rol.Equals(RolEnPizarra.Admin));
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

        public List<PizarraResumenDTO> ObtenerPizarrasDelUsuario(string? idUsuario)
        {
            return _context.PizarraUsuarios.Where(pu => pu.UsuarioId == idUsuario)
                .Include(pu => pu.Pizarra)
                  .Select(pu => new PizarraResumenDTO
                  {
                      Id = pu.Pizarra.Id,
                      FechaCreacion = pu.Pizarra.FechaCreacion,
                      Nombre = pu.Pizarra.NombrePizarra
                  }).ToList();
        }
    }



}
