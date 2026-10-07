using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketDesk.Api.Data;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public NotificacionesController(TicketDeskDbContext context)
    {
        _context = context;
    }

    // GET - OBTENER NOTIFICACIONES DEL USUARIO AUTENTICADO
    [HttpGet]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> ObtenerNotificaciones()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var notificaciones = await _context.Notificaciones
            .Include(n => n.Ticket)
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .Select(n => new
            {
                id = n.Id,
                usuarioId = n.UsuarioId,
                ticketId = n.TicketId,
                ticketTitulo = n.Ticket != null
                    ? n.Ticket.Titulo
                    : null,
                mensaje = n.Mensaje,
                leida = n.Leida,
                fecha = n.Fecha
            })
            .ToListAsync();

        return Ok(notificaciones);
    }

    // PUT - MARCAR UNA NOTIFICACIÓN COMO LEÍDA
    [HttpPut("{id}/leer")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UsuarioId == usuarioId);

        if (notificacion == null)
        {
            return NotFound(new
            {
                mensaje = "La notificación no existe o no pertenece al usuario autenticado."
            });
        }

        notificacion.Leida = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Notificación marcada como leída."
        });
    }

    // PUT - MARCAR TODAS LAS NOTIFICACIONES COMO LEÍDAS
    [HttpPut("leer-todas")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var notificaciones = await _context.Notificaciones
            .Where(n =>
                n.UsuarioId == usuarioId &&
                !n.Leida)
            .ToListAsync();

        foreach (var notificacion in notificaciones)
        {
            notificacion.Leida = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Todas las notificaciones fueron marcadas como leídas.",
            cantidad = notificaciones.Count
        });
    }
}