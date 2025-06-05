using System.Diagnostics;
using System.Drawing;
using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {
        private readonly TextoMemoryService _textoService;
        private readonly TrazoMemoryService _trazoService;
       private readonly IPizarraService _pizarraService;

        public DibujoHub(TrazoMemoryService memoriaService
            , TextoMemoryService textoService, IPizarraService pizarraService)
        {
            _textoService = textoService;
            _trazoService = memoriaService;
            _pizarraService= pizarraService;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var pizarraId = httpContext.Request.Query["pizarraId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);

            //Cargar nombre pizarra al conectarse

            var pizarra= await _pizarraService.ObtenerPizarra(Guid.Parse(pizarraId));
            await Clients.Caller.SendAsync("NombrePizarraCambiado",pizarra.NombrePizarra);

            //si no hay trazos en memoria,cargar base de datos.
            if (!_trazoService.Existe(pizarraId))
            {
                var pizarraGuid = Guid.Parse(pizarraId);
                var trazos = _pizarraService.ObtenerTrazosDeUnaPizarra(pizarraGuid);

                foreach (var trazo in trazos)
                {
                    _trazoService.AgregarTrazo(pizarraId, trazo);
                }
            
            }
            var trazosEnMemoria = _trazoService.ObtenerTrazos(pizarraId);
            await Clients.Caller.SendAsync("CargarTrazos", trazosEnMemoria);

            if(!_textoService.Existe(pizarraId))
            {
                var pizarraGuid = Guid.Parse(pizarraId);
                var textos = _pizarraService.ObtenerTextosDeUnaPizarra(pizarraGuid);

                foreach(var texto in textos)
                {
                    _textoService.AgregarTextoALaPizarra(
                     texto.Id.ToString(),
                     (int)texto.PosX.GetValueOrDefault(),
                     (int)texto.PosY.GetValueOrDefault(),
                     texto.Tamano.GetValueOrDefault(),
                     texto.Contenido,
                    texto.Color,
                     pizarraId
                    );
                }
               

            }
            var textosEnMemoria = _textoService.ObtenerTextos(pizarraId);
            await Clients.Caller.SendAsync("CargarTextos", textosEnMemoria);



            await base.OnConnectedAsync();
        }

        public async Task CambiarNombrePizarra(string pizarraId, string nuevoNombre)
        {

            var pizarraIdGUID = Guid.Parse(pizarraId);

            var pizarra = await _pizarraService.ObtenerPizarra(pizarraIdGUID);

            if (pizarra != null)
            {
                pizarra.NombrePizarra = nuevoNombre;

               _pizarraService.ActualizarPizarra(pizarra);
                await Clients.Group(pizarraId).SendAsync("NombrePizarraCambiado", nuevoNombre);
            }


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
            _trazoService.AgregarTrazo(pizarraId, trazo);

            await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                .SendAsync("ReceivePosition", color, xInicial, yInicial, xFinal, yFinal, tamanioInicial);
        }

        public async Task SendLimpiar(string pizarraId)
        {
            _trazoService.LimpiarPizarra(pizarraId);
            _textoService.LimpiarPizarra(pizarraId);
            await Clients.Group(pizarraId).SendAsync("ReceiveLimpiar");
        }

        public async Task CrearOEditarTexto(string pizarraId, Texto texto)
        {

            var textoEncontrado = await _pizarraService.ObtenerTextoPorId(texto.Id, pizarraId);
               

            if (textoEncontrado != null)
            {
                textoEncontrado.PosX = texto.X;
                textoEncontrado.PosY = texto.Y ;
                textoEncontrado.Tamano = texto.Tamano;
                textoEncontrado.Color= texto.Color;
                texto.Contenido = texto.Contenido;

                _textoService.EditarTextoEnPizarra(textoEncontrado, pizarraId);
            }
            else
            {
                _textoService.AgregarTextoALaPizarra(texto.Id, texto.X, texto.Y, texto.Tamano, texto.Contenido, texto.Color, pizarraId);

              
            }
            await Clients.GroupExcept(pizarraId, Context.ConnectionId).SendAsync("TextoActualizado", texto);

        }


        public async Task MoverTexto(string pizarraId, string id, int x, int y)
        {
            await Clients.Group(pizarraId).SendAsync("TextoMovido", id, x, y);
        }

    }


}
