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
        Task AgregarUsuarioALaPizarra(PizarraUsuario pizarraUsuario);
        Task BorrarTrazosExistentesPizarra(List<Trazo> existentes);
        Task BorrarTextosExistentesPizarra(List<Texto> existentes);
        Task CrearPizarra(Pizarra pizarra);
        Task<bool> EsAdminDeLaPizarra(string idUsuario, Guid id);
        Task<bool> ExisteUsuarioEnPizarra(string id, Guid pizarraId);
        Task<Pizarra> ObtenerPizarra(Guid id);
        List<PizarraResumenDTO> ObtenerPizarrasDelUsuario(string? idUsuario);
        Task<Texto> ObtenerTextoPorId(string idTexto, string pizarraId);
        Task<List<Texto>> ObtenerTextosDeUnaPizarra(Guid pizarraGuid);
        Task<List<Trazo>> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid);
        Task PersistirTextosBD(Dictionary<string, List<Texto>> dictionary);
        Task PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary);
        List<PizarraResumenDTO> ObtenerPizarrasFiltradas(string idUsuario, int? idFiltrarPorRol, string busqueda);
<<<<<<< HEAD
        
=======
        Task<bool> EliminarPizarra(Guid pizarraId);
>>>>>>> ccf910e5840dfb536f2a62339a3619d4dfad8ec8
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

        public Task AgregarUsuarioALaPizarra(PizarraUsuario pizarraUsuario)
        {
            _context.PizarraUsuarios.AddAsync(pizarraUsuario);
            return _context.SaveChangesAsync();

        }

        public async Task BorrarTrazosExistentesPizarra(List<Trazo> existentes)
        {
            _context.Trazos.RemoveRange(existentes);
            await _context.SaveChangesAsync();
        }

        public Task CrearPizarra(Pizarra pizarra)
        {
            _context.Pizarras.Add(pizarra);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public async Task<bool> EsAdminDeLaPizarra(string idUsuario, Guid id)
        {
            return await _context.PizarraUsuarios.AnyAsync(pu => pu.PizarraId == id && pu.UsuarioId == idUsuario
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
                    Nombre = pu.Pizarra.NombrePizarra,
                    Rol = Enum.Parse<RolEnPizarra>(pu.Rol.ToString())
                }).ToList();
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

        public List<PizarraResumenDTO> ObtenerPizarrasFiltradas(string idUsuario, int? idFiltrarPorRol, string busqueda)
        {
            var pizarras = ObtenerPizarrasDelUsuario(idUsuario);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                pizarras = [.. pizarras.Where(p => p.Nombre != null && p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase))];
            }

            if (idFiltrarPorRol.HasValue)
            {
                var rolFiltrado = (RolEnPizarra)idFiltrarPorRol;
                pizarras = rolFiltrado == RolEnPizarra.Admin
                    ? [.. pizarras.Where(p => p.Rol == RolEnPizarra.Admin)]
                    : [.. pizarras.Where(p => p.Rol == RolEnPizarra.Escritura)];
            }

            return pizarras;
        }
<<<<<<< HEAD
=======

        public async Task<bool> EliminarPizarra(Guid pizarraId)
        {
            var pizarra = await _context.Pizarras.FindAsync(pizarraId);
            if (pizarra == null) return false;

            var invitaciones = await _context.InvitacionPizarras
                .Where(i => i.PizarraId == pizarraId)
                .ToListAsync();

            if (invitaciones.Count != 0)
                _context.InvitacionPizarras.RemoveRange(invitaciones);

            _context.Pizarras.Remove(pizarra);
            await _context.SaveChangesAsync();

            return true;
        }
>>>>>>> ccf910e5840dfb536f2a62339a3619d4dfad8ec8
    }
}