using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {
      
        public async Task SendDibujo(string color,int xInicial, int yInicial, int xFinal, int yFinal, int tamanioInicial)
        {
          
            await Clients.Others.SendAsync("ReceivePosition",color, xInicial, yInicial,xFinal,yFinal, tamanioInicial);
        }
        public async Task SendLimpiar()
        {
        
            await Clients.All.SendAsync("ReceiveLimpiar");
        }

        public async Task CrearOEditarTexto(Texto texto)
        {
            await Clients.All.SendAsync("TextoActualizado", texto);
        }

        public async Task MoverTexto(string id, int x, int y)
        {
            await Clients.All.SendAsync("TextoMovido", id, x, y);
        }
    }

    
}
