using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Services.Acciones.Interfaces;

namespace Services
{
    public interface IAccionMemoryService
    {
        void RegistrarAccion(string pizarraId, IAccionPizarra accion);
        Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService);
        Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService);
    }
    public class AccionMemoryService : IAccionMemoryService
    {
        private readonly ConcurrentDictionary<string, Stack<IAccionPizarra>> _accionesUndo = new();
        private readonly ConcurrentDictionary<string, Stack<IAccionPizarra>> _accionesRedo = new();

        public void RegistrarAccion(string pizarraId, IAccionPizarra accion)
        {
            var pilaUndo = _accionesUndo.GetOrAdd(pizarraId, _ => new Stack<IAccionPizarra>());
            pilaUndo.Push(accion);

            if (_accionesRedo.TryGetValue(pizarraId, out var pilaRedo))
            {
                pilaRedo.Clear(); // Al hacer nueva acción, se pierde el camino de Redo
            }
        }

        public async Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            if (!_accionesUndo.TryGetValue(pizarraId, out var pilaUndo) || pilaUndo.Count == 0)
                return;

            var accion = pilaUndo.Pop();
            _accionesRedo.GetOrAdd(pizarraId, _ => new Stack<IAccionPizarra>()).Push(accion);
            await accion.Deshacer(pizarraId, clients, trazoService, textoService);
        }

        public async Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            if (!_accionesRedo.TryGetValue(pizarraId, out var pilaRedo) || pilaRedo.Count == 0)
                return;

            var accion = pilaRedo.Pop();
            _accionesUndo.GetOrAdd(pizarraId, _ => new Stack<IAccionPizarra>()).Push(accion);
            await accion.Rehacer(pizarraId, clients, trazoService, textoService);
        }
    }

   
}
