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
                var textoMemoria = listaTextos.FirstOrDefault(t => t.Id == textoEncontrado.Id);
                if (textoMemoria != null)
                {
                    textoMemoria.PosX = textoEncontrado.PosX;
                    textoMemoria.PosY = textoEncontrado.PosY;
                    textoMemoria.Color = textoEncontrado.Color;
                    textoMemoria.Tamano = textoEncontrado.Tamano;
                    textoMemoria.Contenido = textoEncontrado.Contenido;
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
        public Texto? ObtenerTextoPorIdEnMemoria(string pizarraId, string textoId)
        {
            if (_textosPorPizarra.TryGetValue(pizarraId, out var listaTextos))
            {
                return listaTextos.FirstOrDefault(t => t.Id == textoId);
            }
            return null;
        }

        public void EliminarTexto(string pizarraId, string id)
        {
            if (_textosPorPizarra.TryGetValue(pizarraId, out var listaTextos))
            {
                lock (listaTextos)
                {
                    var textoAEliminar = listaTextos.FirstOrDefault(t => t.Id == id);
                    if (textoAEliminar != null)
                    {
                        listaTextos.Remove(textoAEliminar);
                    }
                }
            }
        }
    }
}
