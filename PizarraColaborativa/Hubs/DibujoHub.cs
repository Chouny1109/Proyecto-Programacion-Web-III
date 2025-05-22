using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var pizarraId = httpContext.Request.Query["pizarraId"];

            if (!string.IsNullOrEmpty(pizarraId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);
            }

            await base.OnConnectedAsync();
        }
        public async Task SendDibujo(string pizarraId, string color, int xInicial, int yInicial, int xFinal, int yFinal, int tamanioInicial)
        {
            await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                .SendAsync("ReceivePosition", color, xInicial, yInicial, xFinal, yFinal, tamanioInicial);
        }

        public async Task SendLimpiar(string pizarraId)
        {
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
