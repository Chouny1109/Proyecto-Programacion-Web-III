using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace PizarraColaborativa.Hubs;

public class NotificacionHub(INotificacionService service, IPizarraUsuarioService puService, UserManager<IdentityUser> userManager) : Hub
{
    private readonly INotificacionService _service = service;
    private readonly IPizarraUsuarioService _puService = puService;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            var historial = await _service.ObtenerHistorialAsync(userId);
            await Clients.Caller.SendAsync("HistorialNotificaciones", historial);

            var notifNoVistas = await _service.ContarNotificacionesNoVistasAsync(userId);
            await Clients.Caller.SendAsync("ActualizarContadorNotificaciones", notifNoVistas);
        }

        await base.OnConnectedAsync();
    }

    public async Task EnviarInvitacion(string remitenteId, string destinatarioNombre, Guid pizarraId,
        string pizarraNombre, int rolId)
    {
        var remitente = await _userManager.FindByIdAsync(remitenteId);
        var destinatario = await _userManager.FindByNameAsync(destinatarioNombre);
        if (destinatario == null) return;

        string titulo = "Nueva invitación";
        string descripcion = $"Has sido invitado a la pizarra '{pizarraNombre}' por el usuario {remitente.UserName}.";

        var dto = await _service.EnviarNotificacionAsync(remitenteId, destinatario.Id, titulo,
            descripcion, pizarraId, pizarraNombre, rolId);
        await Clients.User(dto.DestinatarioId).SendAsync("RecibirNotificacion", dto);
    }

    public async Task ValidarUsuario(string userName, Guid pizarraId)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            await Clients.Caller.SendAsync("ValidacionUsuarioInvitacion", new { existe = false, relacionado = false });
            return;
        }

        bool relacionado = await _puService.ExisteRelacionPizarraUsuarioAsync(user.Id, pizarraId);
        await Clients.Caller.SendAsync("ValidacionUsuarioInvitacion", new { existe = true, relacionado });
    }

    public async Task ResponderInvitacion(Guid notificacionId, string remitenteId, string destinatarioId,
        Guid pizarraId, int rolId, bool aceptada)
    {
        var destinatario = await _userManager.FindByIdAsync(destinatarioId);

        string titulo = aceptada ? "Invitación aceptada" : "Invitación rechazada";
        string descripcion = aceptada
            ? $"El usuario {destinatario.UserName} aceptó tu invitación."
            : $"El usuario {destinatario.UserName} rechazó tu invitación.";

        if (aceptada)
            await _puService.CrearRelacionPizarraUsuarioAsync(destinatarioId, pizarraId, rolId);

        await _service.EliminarNotificacionAsync(notificacionId, destinatarioId);
        await Clients.User(destinatarioId).SendAsync("NotificacionEliminada", notificacionId);

        var dto = await _service.EnviarNotificacionAsync(destinatarioId, remitenteId, titulo, descripcion);
        await Clients.User(remitenteId).SendAsync("RecibirNotificacion", dto);
    }


    public async Task EnviarPizarraEliminada(string remitenteId, string[] destinatariosIds, Guid pizarraId,
        string pizarraNombre)
    {
        var remitente = await _userManager.FindByIdAsync(remitenteId);

        string titulo = "Pizarra eliminada";
        string descripcion = $"La pizarra '{pizarraNombre}' ha sido eliminada por el usuario {remitente.UserName}.";

        foreach (var destinatarioId in destinatariosIds)
        {
            var dto = await _service.EnviarNotificacionAsync(remitenteId, destinatarioId, titulo,
                descripcion, pizarraId, pizarraNombre);
            await Clients.User(dto.DestinatarioId).SendAsync("RecibirNotificacion", dto);
        }
    }

    public async Task EliminarNotificacion(Guid notificacionId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId)) return;

        await _service.EliminarNotificacionAsync(notificacionId, userId);
        var notifNoVistas = await _service.ContarNotificacionesNoVistasAsync(userId);

        await Clients.Group(userId).SendAsync("NotificacionEliminada", notificacionId);
        await Clients.Group(userId).SendAsync("ActualizarContadorNotificaciones", notifNoVistas);
    }

    public async Task VaciarBandeja()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId)) return;

        await _service.VaciarBandejaAsync(userId);

        await Clients.Group(userId).SendAsync("BandejaVaciada");
        await Clients.Group(userId).SendAsync("ActualizarContadorNotificaciones", 0);
    }

    public async Task MarcarTodasComoVistas()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId)) return;

        await _service.MarcarTodasComoVistasAsync(userId);

        var notifNoVistas = await _service.ContarNotificacionesNoVistasAsync(userId);
        await Clients.Caller.SendAsync("ActualizarContadorNotificaciones", notifNoVistas);
    }
}
