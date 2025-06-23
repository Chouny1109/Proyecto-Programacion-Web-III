using Microsoft.AspNetCore.SignalR;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{
    public class AccionTextoMovido : IAccionPizarra
    {
        public string Id { get; }
        public int XAnt { get; }
        public int YAnt { get; }
        public int XNuevo { get; }
        public int YNuevo { get; }

        public AccionTextoMovido(string id, int xAnt, int yAnt, int xNuevo, int yNuevo)
        {
            Id = id;
            XAnt = xAnt;
            YAnt = yAnt;
            XNuevo = xNuevo;
            YNuevo = yNuevo;
        }

        public Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            var texto = textoService.ObtenerTextoPorIdEnMemoria(pizarraId, Id);
            texto.PosX = XAnt;
            texto.PosY = YAnt;
            textoService.EditarTextoEnPizarra(texto, pizarraId);

            return clients.Group(pizarraId).SendAsync("TextoMovido", Id, XAnt, YAnt);
        }

        public Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
        {
            var texto = textoService.ObtenerTextoPorIdEnMemoria(pizarraId, Id);
            texto.PosX = XNuevo;
            texto.PosY = YNuevo;
            textoService.EditarTextoEnPizarra(texto, pizarraId);
            return clients.Group(pizarraId).SendAsync("TextoMovido", Id, XNuevo, YNuevo);
        }
    }
}
