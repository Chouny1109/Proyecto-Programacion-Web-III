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

    }

   
}
