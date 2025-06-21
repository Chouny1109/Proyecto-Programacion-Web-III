using System.Diagnostics;
using System.Drawing;
using Entidades.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Acciones;

namespace PizarraColaborativa.Hubs
{
    public class DibujoHub : Hub
    {
        private static readonly Dictionary<string, (string userId, string pizarraId)> _conexiones
         = new(StringComparer.OrdinalIgnoreCase);
        private readonly ITextoMemoryService _textoService;
        private readonly ITrazoMemoryService _trazoService;
        private readonly IAccionMemoryService _actionsMemoryService;
        private readonly IPizarraService _pizarraService;
        private readonly UserManager<IdentityUser> _userManager;


        public DibujoHub(ITrazoMemoryService memoriaService
            , ITextoMemoryService textoService, IPizarraService pizarraService,
            IAccionMemoryService actionsMemoryService,
           UserManager<IdentityUser> userManager )
        {
            _textoService = textoService;
            _trazoService = memoriaService;
            _pizarraService = pizarraService;
            _actionsMemoryService = actionsMemoryService;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var pizarraId = httpContext.Request.Query["pizarraId"];

            var userId = Context.UserIdentifier;
            Console.WriteLine($"[DEBUG] Conexión ID: {Context.ConnectionId}, UserId: {Context.UserIdentifier}, Pizarra: {pizarraId}");

            _conexiones[Context.ConnectionId] = (userId, pizarraId);

            await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);
            await NotificarUsuariosConectados(pizarraId);

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
        public async Task EliminarTexto(string pizarraId, string id)
        {
            var texto = _textoService.ObtenerTextoPorIdEnMemoria(pizarraId, id);
            if (texto != null)
            {
                _textoService.EliminarTexto(pizarraId, id);

                // Registrar en undo
                _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTextoEliminado(texto));

                await Clients.Group(pizarraId).SendAsync("TextoEliminado", id);
            }
        }

        public async Task CrearOEditarTexto(string pizarraId, Texto texto)
        {

            var textoEncontrado = _textoService.ObtenerTextoPorIdEnMemoria(pizarraId, texto.Id);


            if (textoEncontrado != null)
            {
                var antes = new Entidades.EF.Texto
                {
                    Id = texto.Id,
                    Contenido = textoEncontrado.Contenido,
                    Color = textoEncontrado.Color,
                    Tamano = textoEncontrado.Tamano,
                    PosX = textoEncontrado.PosX,
                    PosY = textoEncontrado.PosY
                };

                if (antes.Contenido != texto.Contenido || antes.Color != texto.Color || antes.Tamano != texto.Tamano)
                {
                    _actionsMemoryService.RegistrarAccion(pizarraId,
                        new AccionTextoEditado(
                            texto.Id,
                            antes.Contenido, texto.Contenido,
                            antes.Color, texto.Color,
                            antes.Tamano ?? 20,
                            texto.Tamano
                        )
                    );
                }

                // Actualizar valores
                textoEncontrado.Contenido = texto.Contenido;
                textoEncontrado.Color = texto.Color;
                textoEncontrado.Tamano = texto.Tamano;
                textoEncontrado.PosX = texto.X;
                textoEncontrado.PosY = texto.Y;

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

        public async Task ObtenerUsuariosDePizarra(string pizarraId)
        {
            var guid = Guid.Parse(pizarraId);
            var usuarios = await _pizarraService.ObtenerUsuariosDePizarra(guid);

            await Clients.Caller.SendAsync("ListaUsuariosPizarra", usuarios);
        }
        public async Task ExpulsarUsuarioDePizarra(string userIdExpulsado, string pizarraId)
        {
            var guid = Guid.Parse(pizarraId);
            await _pizarraService.EliminarUsuarioDePizarra(userIdExpulsado, guid);

         //si esta conectado, se le avisa en tiempo real
            var conexiones = _conexiones
                .Where(c => c.Value.pizarraId == pizarraId && c.Value.userId == userIdExpulsado)
                .Select(c => c.Key)
                .ToList();

            foreach (var conn in conexiones)
            {
                await Clients.Client(conn).SendAsync("UsuarioExpulsado", "Fuiste eliminado de la pizarra.");
                await Groups.RemoveFromGroupAsync(conn, pizarraId);
            }
            ///
           
            //lista actualziada con los usuarios
            var usuariosActualizados = await _pizarraService.ObtenerUsuariosDePizarra(guid);
            await Clients.Caller.SendAsync("ListaUsuariosPizarra", usuariosActualizados);
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
                textoMemoria.PosX = xFinal;
                textoMemoria.PosY = yFinal;

                _textoService.EditarTextoEnPizarra(textoMemoria, pizarraId);

                _actionsMemoryService.RegistrarAccion(pizarraId, new AccionTextoMovido(id, xAnt, yAnt, xFinal, yFinal));


                await Clients.Group(pizarraId).SendAsync("TextoMovido", id, xFinal, yFinal);
            }

        }
        public async Task EnviarImagen(string pizarraId, string base64, int posX, int posY, string idImg)
        {

            await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                .SendAsync("RecibirImagen", base64, posX, posY, idImg);
        }

        public async Task MoverImagen(string pizarraId, string idImg, int posX, int posY)
        {
            try
            {
                await Clients.GroupExcept(pizarraId, Context.ConnectionId)
                    .SendAsync("ActualizarPosicionImagen", idImg, posX, posY);
            }
            catch (Exception ex)
            {
                // Loggear ex.Message si tenés un logger
                throw new HubException("Error en MoverImagen: " + ex.Message);
            }
        }

        public async Task CerrarImagen(string idPizarra, string idImg)
        {
            await Clients.GroupExcept(idPizarra, Context.ConnectionId)
                    .SendAsync("SacarImagen", idImg);
        }



        private bool EstaCercaDelPunto(Trazo t, int x, int y, int radio)
        {
            int dx = (int)t.Xinicio.GetValueOrDefault() - x;
            int dy = (int)t.Yinicio.GetValueOrDefault() - y;
            return (dx * dx + dy * dy) <= (radio * radio);
        }


        private async Task NotificarUsuariosConectados(string pizarraId)
        {
            var usuariosConectados = _conexiones
                .Where(c => c.Value.pizarraId == pizarraId)
                .Select(c => c.Value.userId)
                .Distinct()
                .ToList();

            // Obtener nombres desde BD
            var lista = await _userManager.Users
                .Where(u => usuariosConectados.Contains(u.Id))
                .Select(u => new { userId = u.Id, userName = u.UserName })
                .ToListAsync();

            await Clients.Group(pizarraId).SendAsync("UsuariosConectados", lista);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_conexiones.TryGetValue(Context.ConnectionId, out var datos))
            {
                _conexiones.Remove(Context.ConnectionId);
                await NotificarUsuariosConectados(datos.pizarraId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }




}
