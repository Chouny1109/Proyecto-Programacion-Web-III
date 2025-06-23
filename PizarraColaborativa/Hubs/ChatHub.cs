using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs;

public class ChatHub(IMensajeService service, UserManager<IdentityUser> userManager) : Hub
{
    private readonly IMensajeService _service = service;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var pizarraId = Context.GetHttpContext()?.Request.Query["pizarraId"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(pizarraId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, pizarraId);
            await Clients.Group(pizarraId).SendAsync("UsuarioConectado", userId, user?.UserName);

            var mensajes = await _service.ObtenerMensajesAsync(Guid.Parse(pizarraId), userId);
            await Clients.Caller.SendAsync("HistorialMensajes", pizarraId, mensajes);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var pizarraId = Context.GetHttpContext()?.Request.Query["pizarraId"].FirstOrDefault();

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(pizarraId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            await Clients.Group(pizarraId).SendAsync("UsuarioDesconectado", userId, user?.UserName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EnviarMensaje(string pizarraId, string mensaje)
    {
        var userId = Context.UserIdentifier;
        var user = await _userManager.FindByIdAsync(userId);

        await _service.GuardarMensajeAsync(userId, Guid.Parse(pizarraId), user.UserName, mensaje);
        await Clients.Group(pizarraId).SendAsync("RecibirMensaje", user.UserName, mensaje, pizarraId);

        var usuariosIdsDelChat = await _service.ObtenerUsuariosDelChatAsync(Guid.Parse(pizarraId));
        foreach (var destinatarioId in usuariosIdsDelChat)
        {
            if (destinatarioId != userId)
            {
                int msgNoVistos = await _service.CantidadMensajesNoVistosAsync(destinatarioId, Guid.Parse(pizarraId));
                await Clients.User(destinatarioId).SendAsync("ActualizarContadorMensajes", pizarraId, msgNoVistos);
            }
        }
    }

    public async Task MarcarTodosComoVistos(string pizarraId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId)) return;

        await _service.MarcarTodosLosMensajesComoVistosAsync(userId, Guid.Parse(pizarraId));

        await Clients.User(userId).SendAsync("ActualizarContadorMensajes", pizarraId, 0);
    }

    public async Task UsuarioEscribiendo(string pizarraId)
    {
        var userId = Context.UserIdentifier;
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
