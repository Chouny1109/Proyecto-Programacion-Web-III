using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;
using Microsoft.Extensions.Primitives;

namespace Services
{
    public class TrazoMemoryService
    {
        private readonly ConcurrentDictionary<string, List<Trazo>> _trazosPorPizarra = new();

        public void AgregarTrazo(string pizarraId, Trazo trazo)
        {
            var lista = _trazosPorPizarra.GetOrAdd(pizarraId, _ => new List<Trazo>());
            lock (lista)
            {
                lista.Add(trazo);
            }
        }

        public List<Trazo> ObtenerTrazos(string pizarraId)
        {
            return _trazosPorPizarra.TryGetValue(pizarraId, out var lista)
                ? new List<Trazo>(lista) // Evita exponer la lista interna
                : new List<Trazo>();
        }


        public Dictionary<string, List<Trazo>> ObtenerTodas()
        {
            return _trazosPorPizarra.ToDictionary(kvp => kvp.Key, kvp => new List<Trazo>(kvp.Value));
        }

        public bool Existe(string pizarraId)
        {
          return _trazosPorPizarra.ContainsKey(pizarraId);
        }

        public void LimpiarPizarra(string pizarraId)
        {
            _trazosPorPizarra.TryRemove(pizarraId, out _);
        }
    }

}