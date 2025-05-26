using System.Collections.Concurrent;
using Entidades.EF;
using Microsoft.Extensions.Primitives;

namespace Services
{
    public interface IPizarraService
    {

        Task CrearPizarra(Pizarra pizarra);
        Task<Pizarra> ObtenerPizarra(Guid id);
 
    }
    public class PizarraService : IPizarraService
    {
        private readonly ConcurrentDictionary<string, List<Trazo>> _trazosPorPizarra = new();

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

        public async Task<Pizarra> ObtenerPizarra(Guid id)
        {
            var pizarra = await _context.Pizarras.FindAsync(id);
            return pizarra;
        }

    }



}
