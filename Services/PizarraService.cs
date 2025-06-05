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
        void ActualizarPizarra(Pizarra pizarra);
        void AgregarTrazo(Trazo trazo);
        Task AgregarUsuarioALaPizarra(PizarraUsuario pizarraUsuario);
        void BorrarTrazosExistentesPizarra(List<Trazo> existentes);
        Task CrearPizarra(Pizarra pizarra);
        Task<bool> EsAdminDeLaPizarra(string idUsuario, Guid id);
        Task<bool> ExisteUsuarioEnPizarra(string id, Guid pizarraId);
        Task<Pizarra> ObtenerPizarra(Guid id);
        List<PizarraResumenDTO> ObtenerPizarrasDelUsuario(string? idUsuario);
        Task<Texto> ObtenerTextoPorId(string idTexto, string pizarraId);
        List<Texto> ObtenerTextosDeUnaPizarra(Guid pizarraGuid);
        List<Trazo> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid);
        void PersistirTextosBD(Dictionary<string, List<Texto>> dictionary);
        void PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary);
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
            _context.SaveChangesAsync();
            return Task.CompletedTask;
        }

        public void BorrarTrazosExistentesPizarra(List<Trazo> existentes)
        {
            _context.Trazos.RemoveRange(existentes);
            _context.SaveChanges();
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
           return  await _context.Textos.FirstOrDefaultAsync(t => t.Id.Equals(idTexto)
            && t.PizarraId.Equals(pizarraId));

        }

        public void PersistirTextosBD(Dictionary<string, List<Texto>> dictionary)
        {
            foreach(var (pizarraId,textos) in dictionary)
            {
                var pizarraguid = Guid.Parse(pizarraId);
                var existentes = ObtenerTextosDeUnaPizarra(pizarraguid);
                BorrarTextosExistentesPizarra(existentes);

                foreach (var texto in textos)
                {
                    texto.Id = texto.Id;
                    texto.PizarraId = pizarraguid;
                    AgregarTexto(texto);
                }
            }
            _context.SaveChanges();
        }

        private void BorrarTextosExistentesPizarra(List<Texto> existentes)
        {
            _context.Textos.RemoveRange(existentes);
            _context.SaveChanges();
        }

        private void AgregarTexto(Texto texto)
        {
            _context.Textos.Add(texto);
            _context.SaveChanges();
        }

        public List<Texto> ObtenerTextosDeUnaPizarra(Guid pizarraguid)
        {
            return _context.Textos.Where(t => t.PizarraId==pizarraguid).ToList();
        }

        public void PersistirTrazosBD(Dictionary<string, List<Trazo>> dictionary)
        {

            foreach (var (pizarraId, trazos) in dictionary)
            {
                var pizarraguid = Guid.Parse(pizarraId);
                var existentes = ObtenerTrazosDeUnaPizarra(pizarraguid);
                BorrarTrazosExistentesPizarra(existentes);

                foreach (var trazo in trazos)
                {
                    trazo.Id = 0;
                    trazo.PizarraId = pizarraguid;
                    AgregarTrazo(trazo);
                }
               
        }
            _context.SaveChanges();
        }

        public List<Trazo> ObtenerTrazosDeUnaPizarra(Guid pizarraGuid)
        {
            return _context.Trazos.Where(t => t.PizarraId == pizarraGuid).ToList();
        }

        public void ActualizarPizarra(Pizarra pizarra)
        {
           _context.Pizarras.Update(pizarra);
            _context.SaveChangesAsync();
        }

      
    }


}
