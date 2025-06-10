using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs;

public class ChatHub(IPizarraService service, UserManager<IdentityUser> userManager) : Hub
{
    private readonly IPizarraService _service = service;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private async Task<string?> ObtenerUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
        var pizarras = _service.ObtenerPizarrasChat(userId);
        var userName = await ObtenerUserNameAsync(userId);

        foreach (var pizarra in pizarras)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pizarra.Nombre);
            await Clients.Group(pizarra.Nombre).SendAsync("UsuarioConectado", userId, userName);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
        var pizarras = _service.ObtenerPizarrasChat(userId);
        var userName = await ObtenerUserNameAsync(userId);

        foreach (var pizarra in pizarras)
        {
            await Clients.Group(pizarra.Nombre).SendAsync("UsuarioDesconectado", userId, userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EnviarMensaje(string pizarraId, string userId, string mensaje)
    {
        var userName = await ObtenerUserNameAsync(userId);
        await Clients.Group(pizarraId).SendAsync("RecibirMensaje", userId, userName, mensaje, pizarraId);
    }

    public async Task UsuarioEscribiendo(string pizarraId, string userId)
    {
        var userName = await ObtenerUserNameAsync(userId);
        await Clients.Group(pizarraId).SendAsync("UsuarioEscribiendo", userName, pizarraId);
    }

    public async Task UsuarioDejoDeEscribir(string pizarraId, string userId)
    {
        await Clients.Group(pizarraId).SendAsync("UsuarioDejoDeEscribir", userId, pizarraId);
    }
}