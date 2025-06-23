using Microsoft.AspNetCore.SignalR;

namespace Services.Acciones.Interfaces
{
    public interface IAccionPizarra
    {
        Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService);
        Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService);
    }
}
