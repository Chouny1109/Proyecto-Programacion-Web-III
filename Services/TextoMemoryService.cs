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
    public class TextoMemoryService
    {
        private readonly ConcurrentDictionary<string, List<Texto>> _textosPorPizarra = new();

        public void AgregarTextoALaPizarra(string id, int x, int y, int tamano, string? contenido, string? color,string pizarraId)
        {
            var texto = new Texto()
            {
                Id = id,
                Color = color, 
                PosX = x,
                PosY = y,
                Tamano = tamano,
                Contenido = contenido,
            };

            var lista = _textosPorPizarra.GetOrAdd(pizarraId, _ => new List<Texto>());
            lock (lista)
            {
                lista.Add(texto);
            }
        }

        public void EditarTextoEnPizarra(Texto textoEncontrado, string pizarraId)
        {
            if (_textosPorPizarra.TryGetValue(pizarraId, out var listaTextos))
            {
                var index = listaTextos.FindIndex(t => t.Id == textoEncontrado.Id);
                if (index != -1)
                {
                    listaTextos[index] = textoEncontrado; 
                }
            }
        }

        public bool Existe(string pizarraId)
        { 
            return _textosPorPizarra.ContainsKey(pizarraId);
        }

        public void LimpiarPizarra(string pizarraId)
        {
            _textosPorPizarra.TryRemove(pizarraId, out _);
        }

        public List<Texto> ObtenerTextos(string pizarraId)
        {
            return _textosPorPizarra.TryGetValue(pizarraId, out var lista)
                ? new List<Texto>(lista) : new List<Texto>();
        }

        public Dictionary<string, List<Texto>> ObtenerTodas()
        {
            return _textosPorPizarra.ToDictionary(kvp => kvp.Key, 
                kvp => new List<Texto>(kvp.Value));
        }
    }
}
