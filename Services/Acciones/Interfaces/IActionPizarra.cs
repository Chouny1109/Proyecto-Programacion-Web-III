using Microsoft.AspNetCore.SignalR;
using Services;

namespace Services.Acciones.Interfaces
{
  
        public interface IAccionPizarra
        {
            Task Deshacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService);
            Task Rehacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService);
        }

    }

