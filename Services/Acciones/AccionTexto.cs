using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{ 
    public class AccionTexto : IAccionPizarra
    {
        public Texto Texto { get; set; }
        public AccionTexto(Texto texto)

        {
            Texto = texto;
        }
        public Task Deshacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService)
        {
            textoService.EliminarTexto(pizarraId, Texto.Id);
            return clients.Group(pizarraId).SendAsync("TextoEliminado", Texto.Id);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, TrazoMemoryService trazoService, TextoMemoryService textoService)
        {
            textoService.AgregarTextoALaPizarra(Texto.Id,(int)Texto.PosX, (int)Texto.PosY, Texto.Tamano ?? 20, Texto.Contenido, Texto.Color, pizarraId);
            return clients.Group(pizarraId).SendAsync("TextoActualizado", Texto);
        }
    }
}
