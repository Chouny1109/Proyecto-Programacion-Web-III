using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{
    public class AccionTextoEliminado : IAccionPizarra
    {
        public Texto Texto { get; set; }

        public AccionTextoEliminado(Texto texto)
        {
            Texto = texto;
        }

        public Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            textoService.AgregarTextoALaPizarra(
                Texto.Id,
                (int)Texto.PosX,
                (int)Texto.PosY,
                Texto.Tamano ?? 20,
                Texto.Contenido,
                Texto.Color,
                pizarraId);

            return clients.Group(pizarraId).SendAsync("TextoActualizado", Texto);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            textoService.EliminarTexto(pizarraId, Texto.Id);
            return clients.Group(pizarraId).SendAsync("TextoEliminado", Texto.Id);
        }
    }
}
