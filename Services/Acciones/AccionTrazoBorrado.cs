using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{
    public class AccionTrazoBorrado : IAccionPizarra
    {
        public Guid GrupoTrazoId { get; }
        public List<Trazo> Segmentos { get; }

        public AccionTrazoBorrado(List<Trazo> segmentos)
        {
            Segmentos = segmentos;
            GrupoTrazoId = segmentos.First().GrupoTrazoId ?? Guid.NewGuid();
        }

        public Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            foreach (var trazo in Segmentos)
                trazoService.AgregarTrazo(pizarraId, trazo);

            return clients.Group(pizarraId).SendAsync("DibujarTrazoCompleto", Segmentos);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            trazoService.EliminarTrazo(pizarraId, GrupoTrazoId);
            var trazosActuales = trazoService.ObtenerTrazos(pizarraId);
            return clients.Group(pizarraId).SendAsync("CargarTrazos", trazosActuales);
        }
    }
}
