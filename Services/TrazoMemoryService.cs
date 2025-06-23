using System.Collections.Concurrent;
using Entidades.EF;

namespace Services
{
    public interface ITrazoMemoryService
    {
        void AgregarTrazo(string pizarraId, Trazo trazo);
        List<Trazo> ObtenerTrazos(string pizarraId);
        Dictionary<string, List<Trazo>> ObtenerTodas();
        bool Existe(string pizarraId);
        void LimpiarPizarra(string pizarraId);
        void EliminarTrazo(string pizarraId, Guid grupoTrazoId);
    }

    public class TrazoMemoryService : ITrazoMemoryService
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

        public void EliminarTrazo(string pizarraId, Guid grupoTrazoId)
        {
            if (_trazosPorPizarra.TryGetValue(pizarraId, out var lista))
            {
                lock (lista)
                {
                    lista.RemoveAll(t => t.GrupoTrazoId == grupoTrazoId);
                }
            }
        }
    }
}
