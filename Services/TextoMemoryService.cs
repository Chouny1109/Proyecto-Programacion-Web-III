using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;

namespace Services
{
    public class TextoMemoryService
    {
        private readonly ConcurrentDictionary<string, List<Texto>> _textosPorPizarra = new();

        public void AgregarTextoALaPizarra(string id, int x, int y, int tamano, string? contenido, string? color)
        {
            throw new NotImplementedException();
        }

        public void EditarTextoEnPizarra(Texto textoEncontrado, string pizarraId)
        {
            throw new NotImplementedException();
        }
    }
}
