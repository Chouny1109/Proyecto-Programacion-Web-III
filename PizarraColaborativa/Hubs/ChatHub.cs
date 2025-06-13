using System.Security.Claims;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs;

public class ChatHub(IPizarraService service, UserManager<IdentityUser> userManager) : Hub
{
    private readonly IPizarraService _service = service;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pizarraId = Context.GetHttpContext()?.Request.Query["pizarraId"].ToString();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(pizarraId)) return;

        var user = await _userManager.FindByIdAsync(userId);
        await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);
        await Clients.Group(pizarraId).SendAsync("UsuarioConectado", userId, user?.UserName);

        var mensajes = await _service.ObtenerMensajesAsync(Guid.Parse(pizarraId), userId);
        await Clients.Caller.SendAsync("HistorialMensajes", pizarraId, mensajes);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pizarraId = Context.GetHttpContext()?.Request.Query["pizarraId"].ToString();

        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(pizarraId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            await Clients.Group(pizarraId).SendAsync("UsuarioDesconectado", userId, user.UserName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EnviarMensaje(string pizarraId, string mensaje)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(pizarraId)) return;

        var user = await _userManager.FindByIdAsync(userId);

        var mensajeGuardado = await _service.GuardarMensajeAsync(new MensajeDTO
        {
            PizarraId = Guid.Parse(pizarraId),
            UsuarioId = userId,
            NombreUsuario = user.UserName,
            Descripcion = mensaje
        });

        await Clients.Group(pizarraId).SendAsync("RecibirMensaje", userId, user.UserName, mensajeGuardado.Descripcion, pizarraId);
    }

    public async Task MarcarTodosComoVistos(string pizarraId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return;

        await _service.MarcarTodosLosMensajesComoVistosAsync(Guid.Parse(pizarraId), userId);
    }

    public async Task UsuarioEscribiendo(string pizarraId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(pizarraId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            await Clients.Group(pizarraId).SendAsync("UsuarioEscribiendo", user?.UserName, pizarraId);
        }
    }
    
    public async Task UsuarioDejoDeEscribir(string pizarraId)
    {
        if (string.IsNullOrWhiteSpace(pizarraId)) return;
        await Clients.Group(pizarraId).SendAsync("UsuarioDejoDeEscribir", pizarraId);
    }
}
