using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComentariosTicketsController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public ComentariosTicketsController(TicketDeskDbContext context)
    {
        _context = context;
    }

    // POST - AGREGAR COMENTARIO
    [HttpPost("{ticketId}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> CrearComentario(
        int ticketId,
        [FromBody] CreateComentarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el ticket exista
        var ticket = await _context.Tickets
            .FindAsync(ticketId);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        // Obtener el usuario autenticado desde el JWT
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

        // Obtener el rol del usuario
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        // Usuario solamente puede comentar sus propios tickets
        if (rol == "Usuario" && ticket.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        // Agente solamente puede comentar tickets asignados a él
        if (rol == "Agente" && ticket.AgenteId != usuarioId)
        {
            return Forbid();
        }

        // Crear comentario
        var comentario = new ComentarioTicket
        {
            TicketId = ticketId,
            UsuarioId = usuarioId,
            Contenido = dto.Contenido,
            Fecha = DateTime.Now
        };

        _context.ComentariosTickets.Add(comentario);

        await _context.SaveChangesAsync();

        // Obtener el nombre del usuario
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        return Ok(new
        {
            mensaje = "Comentario agregado correctamente.",
            id = comentario.Id,
            ticketId = comentario.TicketId,
            usuarioId = comentario.UsuarioId,
            usuarioNombre = usuario?.Nombre ?? "Usuario desconocido",
            contenido = comentario.Contenido,
            fecha = comentario.Fecha
        });
    }

    // GET - OBTENER COMENTARIOS DE UN TICKET
    [HttpGet("{ticketId}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> ObtenerComentarios(int ticketId)
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

        // Obtener usuario autenticado
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

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        // Usuario solamente puede ver comentarios de sus propios tickets
        if (rol == "Usuario" && ticket.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        // Agente solamente puede ver comentarios de tickets asignados a él
        if (rol == "Agente" && ticket.AgenteId != usuarioId)
        {
            return Forbid();
        }

        var comentarios = await _context.ComentariosTickets
            .Include(c => c.Usuario)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.Fecha)
            .Select(c => new
            {
                id = c.Id,
                ticketId = c.TicketId,
                usuarioId = c.UsuarioId,
                usuarioNombre = c.Usuario != null
                    ? c.Usuario.Nombre
                    : "Usuario desconocido",
                contenido = c.Contenido,
                fecha = c.Fecha
            })
            .ToListAsync();

        return Ok(comentarios);
    }
}