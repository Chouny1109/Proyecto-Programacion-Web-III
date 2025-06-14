using System.Diagnostics;
using System.Drawing;
using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Acciones;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {
        private readonly ITextoMemoryService _textoService;
        private readonly ITrazoMemoryService _trazoService;
        private readonly IAccionMemoryService _actionsMemoryService;
        private readonly IPizarraService _pizarraService;
       

        public DibujoHub(ITrazoMemoryService memoriaService
            , ITextoMemoryService textoService, IPizarraService pizarraService,
            IAccionMemoryService actionsMemoryService)
        {
            _textoService = textoService;
            _trazoService = memoriaService;
            _pizarraService = pizarraService;
            _actionsMemoryService = actionsMemoryService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var pizarraId = httpContext.Request.Query["pizarraId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);

            //Cargar nombre pizarra al conectarse

            var pizarra = await _pizarraService.ObtenerPizarra(Guid.Parse(pizarraId));
            await Clients.Caller.SendAsync("NombrePizarraCambiado", pizarra.NombrePizarra);

            //Cargar Color de fondo pizarra al conectarse
            var colorFondo = await _pizarraService.ObtenerColorFondoDeUnaPizarra(Guid.Parse(pizarraId)) ?? "#ffffff";
            await Clients.Caller.SendAsync("ColorFondoCambiado", colorFondo);

            //si no hay trazos en memoria,cargar base de datos.
            if (!_trazoService.Existe(pizarraId))
            {
                var pizarraGuid = Guid.Parse(pizarraId);
                var trazos = await _pizarraService.ObtenerTrazosDeUnaPizarra(pizarraGuid);

                foreach (var trazo in trazos)
                {
                    trazo.Pizarra = null; // Evitar circular reference
                    _trazoService.AgregarTrazo(pizarraId, trazo);
                }

            }

            var trazosEnMemoria = _trazoService.ObtenerTrazos(pizarraId);
            await Clients.Caller.SendAsync("CargarTrazos", trazosEnMemoria);

            if (!_textoService.Existe(pizarraId))
            {
                var pizarraGuid = Guid.Parse(pizarraId);
                var textos = await _pizarraService.ObtenerTextosDeUnaPizarra(pizarraGuid);

                foreach (var texto in textos)
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
        public async Task BorrarTrazosEnRango(string pizarraId, int x, int y, int radio)
        {
            var trazos = _trazoService.ObtenerTrazos(pizarraId);

            // Detectamos trazos cuyo punto inicial esté dentro del rango
            var gruposParaBorrar = trazos
                .Where(t => EstaCercaDelPunto(t, x, y, radio))
                .Select(t => t.GrupoTrazoId)
                .Where(g => g.HasValue)
                .Distinct()
                .ToList();

            foreach (var grupoId in gruposParaBorrar)
            {
                var trazosDelGrupo = trazos.Where(t => t.GrupoTrazoId == grupoId).ToList();

                _trazoService.EliminarTrazo(pizarraId, grupoId.Value);

                // Registrar acción para Undo
                _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTrazoBorrado(trazosDelGrupo));
            }

            var trazosActuales = _trazoService.ObtenerTrazos(pizarraId);
            await Clients.Group(pizarraId).SendAsync("CargarTrazos", trazosActuales);
        }


        public async Task CambiarColorFondo(string pizarraId, string colorFondo)
        {
            await _pizarraService.CambiarColorFondoPizarra(pizarraId, colorFondo);
            await Clients.Group(pizarraId).SendAsync("ColorFondoCambiado", colorFondo);
        }
        public async Task CambiarNombrePizarra(string pizarraId, string nuevoNombre)
        {

            var pizarraIdGUID = Guid.Parse(pizarraId);
            await _pizarraService.ActualizarPizarra(pizarraIdGUID, nuevoNombre);
            await Clients.Group(pizarraId).SendAsync("NombrePizarraCambiado", nuevoNombre);

        }



        public async Task EnviarTrazoCompleto(string pizarraId, List<Trazo> segmentos, Guid grupoTrazoId)
        {
            foreach (var trazo in segmentos)
            {
                trazo.GrupoTrazoId = grupoTrazoId;
                _trazoService.AgregarTrazo(pizarraId, trazo);
            }

            _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTrazo(segmentos));

            await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                .SendAsync("DibujarTrazoCompleto", segmentos);
        }


        public async Task SendLimpiar(string pizarraId)
        {
            _trazoService.LimpiarPizarra(pizarraId);
            _textoService.LimpiarPizarra(pizarraId);

            var pizarraGUID = Guid.Parse(pizarraId);
            var textosExistentesEnBD = await _pizarraService.ObtenerTextosDeUnaPizarra(pizarraGUID);
            var trazosExistentesEnBD = await _pizarraService.ObtenerTrazosDeUnaPizarra(pizarraGUID);

            await _pizarraService.BorrarTrazosExistentesPizarra(trazosExistentesEnBD);
            await _pizarraService.BorrarTextosExistentesPizarra(textosExistentesEnBD);
            await _pizarraService.SetColorBlancoPizarra(pizarraGUID);



            await Clients.Group(pizarraId).SendAsync("ReceiveLimpiar");
        }

        public async Task CrearOEditarTexto(string pizarraId, Texto texto)
        {

            var textoEncontrado = await _pizarraService.ObtenerTextoPorId(texto.Id, pizarraId);


            if (textoEncontrado != null)
            {
                textoEncontrado.PosX = texto.X;
                textoEncontrado.PosY = texto.Y;
                textoEncontrado.Tamano = texto.Tamano;
                textoEncontrado.Color = texto.Color;
                textoEncontrado.Contenido = texto.Contenido;

                _textoService.EditarTextoEnPizarra(textoEncontrado, pizarraId);
            }
            else
            {
                _textoService.AgregarTextoALaPizarra(texto.Id, texto.X, texto.Y, texto.Tamano, texto.Contenido, texto.Color, pizarraId);
                _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTexto(new Entidades.EF.Texto
                {
                    Id = texto.Id,
                    PosX = texto.X,
                    PosY = texto.Y,
                    Tamano = texto.Tamano,
                    Contenido = texto.Contenido,
                    Color = texto.Color
                }));


            }
            await Clients.GroupExcept(pizarraId, Context.ConnectionId).SendAsync("TextoActualizado", texto);

        }

        public async Task SolicitarTrazos(string pizarraId)
        {
            var trazos = _trazoService.ObtenerTrazos(pizarraId);
            await Clients.Caller.SendAsync("CargarTrazos", trazos);
        }

        public async Task DeshacerUltimaAccion(string pizarraId)
        {
            await _actionsMemoryService.Deshacer(pizarraId, Clients, _trazoService, _textoService);
        }

        public async Task RehacerUltimaAccion(string pizarraId)
        {
            await _actionsMemoryService.Rehacer(pizarraId, Clients, _trazoService, _textoService);
        }

        public async Task MoverTexto(string pizarraId, string id, int xAnt, int yAnt, int xFinal, int yFinal)
        {

            var textoMemoria = _textoService.ObtenerTextoPorIdEnMemoria(pizarraId, id);
            if (textoMemoria != null)
            {
               
                _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTextoMovido(id, xAnt, yAnt, xFinal, yFinal));


                await Clients.Group(pizarraId).SendAsync("TextoMovido", id, xFinal, yFinal);
            }

        }

        private bool EstaCercaDelPunto(Trazo t, int x, int y, int radio)
        {
            int dx = (int)t.Xinicio.GetValueOrDefault() - x;
            int dy = (int)t.Yinicio.GetValueOrDefault() - y;
            return (dx * dx + dy * dy) <= (radio * radio);
        }


    }




}
