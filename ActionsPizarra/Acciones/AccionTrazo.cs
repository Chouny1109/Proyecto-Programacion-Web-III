using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using PizarraColaborativa.Hubs.Acciones.Interfaces;
using Services;

namespace ActionsPizarra.Acciones
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
           // trazoService.EliminarTrazo(pizarraId, Trazo); 
            return clients.Group(pizarraId).SendAsync("TrazoEliminado", Trazo);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService)
        {
            trazoService.AgregarTrazo(pizarraId, Trazo);
            return clients.Group(pizarraId).SendAsync("TrazoRehecho", Trazo);
        }
    }
}
