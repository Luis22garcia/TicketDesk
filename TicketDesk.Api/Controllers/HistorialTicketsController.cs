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
public class HistorialTicketsController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public HistorialTicketsController(TicketDeskDbContext context)
    {
        _context = context;
    }

    // GET - OBTENER HISTORIAL DE UN TICKET
    [HttpGet("{ticketId}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> GetHistorial(int ticketId)
    {
        // Verificar que el ticket exista
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        // Obtener el ID del usuario autenticado desde el JWT
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

        // Obtener el rol
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        // ADMINISTRADOR puede consultar cualquier historial
        if (rol == "Administrador")
        {
            // Permitido
        }
        // USUARIO solamente puede consultar sus propios tickets
        else if (rol == "Usuario")
        {
            if (ticket.UsuarioId != usuarioId)
            {
                return Forbid();
            }
        }
        // AGENTE solamente puede consultar tickets asignados a él
        else if (rol == "Agente")
        {
            if (ticket.AgenteId != usuarioId)
            {
                return Forbid();
            }
        }
        else
        {
            return Forbid();
        }

        // Obtener historial
        var historial = await _context.HistorialTickets
            .Include(h => h.Usuario)
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.Fecha)
            .Select(h => new
            {
                id = h.Id,
                ticketId = h.TicketId,
                accion = h.Accion,
                estadoAnterior = h.EstadoAnterior,
                estadoNuevo = h.EstadoNuevo,
                comentario = h.Comentario,
                usuarioId = h.UsuarioId,
                usuarioNombre = h.Usuario != null
                    ? h.Usuario.Nombre
                    : "Usuario desconocido",
                fecha = h.Fecha
            })
            .ToListAsync();

        return Ok(historial);
    }
}