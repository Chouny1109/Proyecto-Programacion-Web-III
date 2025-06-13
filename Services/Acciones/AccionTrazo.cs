using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{
    public class AccionTrazo : IAccionPizarra
    {
        public Trazo Trazo { get;}

        public AccionTrazo(Trazo trazo)
        {
            Trazo = trazo;   
        }

        public Task Deshacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService)
        {
            trazoService.EliminarTrazo(pizarraId, Trazo.Id); 
            return clients.Group(pizarraId).SendAsync("TrazoEliminado", Trazo);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService)
        {
            trazoService.AgregarTrazo(pizarraId, Trazo);
            return clients.Group(pizarraId).SendAsync("TrazoRehecho", Trazo);
        }
    }
}
