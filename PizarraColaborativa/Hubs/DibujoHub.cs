using System.Diagnostics;
using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {
        private readonly TrazoMemoryService _service;
        private readonly IServiceProvider _serviceProvider;

        public DibujoHub(TrazoMemoryService memoriaService, IServiceProvider serviceProvider)
        {
           _service = memoriaService;
            _serviceProvider = serviceProvider;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var pizarraId = httpContext.Request.Query["pizarraId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);

            //si no hay trazos en memoria,cargar base de datos.
            if (!_service.Existe(pizarraId))
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProyectoPizarraContext>();

                var pizarraGuid = Guid.Parse(pizarraId); 
                var trazos = context.Trazos.Where(t => t.PizarraId == pizarraGuid).ToList();

                foreach (var trazo in trazos)
                {
                    _service.AgregarTrazo(pizarraId, trazo);
                }
            
            }
            var trazosEnMemoria = _service.ObtenerTrazos(pizarraId);
            await Clients.Caller.SendAsync("CargarTrazos", trazosEnMemoria);
           

            await base.OnConnectedAsync();
        }
        public async Task SendDibujo(string pizarraId, string color, int xInicial, int yInicial, int xFinal, int yFinal, int tamanioInicial)
        {
            var trazo = new Trazo
            {
                Color = color,
                Xinicio = xInicial,
                Yinicio = yInicial,
                Xfin = xFinal,
                Yfin = yFinal,
                Grosor = tamanioInicial
            };
            _service.AgregarTrazo(pizarraId, trazo);

            await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                .SendAsync("ReceivePosition", color, xInicial, yInicial, xFinal, yFinal, tamanioInicial);
        }

        public async Task SendLimpiar(string pizarraId)
        {
            _service.LimpiarPizarra(pizarraId);
            await Clients.Group(pizarraId).SendAsync("ReceiveLimpiar");
        }

        public async Task CrearOEditarTexto(string pizarraId, Texto texto)
        {
            await Clients.Group(pizarraId).SendAsync("TextoActualizado", texto);
        }


        public async Task MoverTexto(string pizarraId, string id, int x, int y)
        {
            await Clients.Group(pizarraId).SendAsync("TextoMovido", id, x, y);
        }

    }


}
