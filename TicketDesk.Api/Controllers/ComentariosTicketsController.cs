
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

    // MÉTODO AUXILIAR - OBTENER EL ID DEL USUARIO AUTENTICADO
    private bool TryGetAuthenticatedUserId(out int usuarioId)
    {
        var usuarioIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(usuarioIdClaim))
        {
            usuarioIdClaim =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        return int.TryParse(usuarioIdClaim, out usuarioId);
    }

    // MÉTODO AUXILIAR - OBTENER EL ROL DEL USUARIO
    private string? ObtenerRol()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    // MÉTODO AUXILIAR - VERIFICAR PERMISOS SOBRE EL TICKET
    private bool TienePermisoSobreTicket(
        Ticket ticket,
        string? rol,
        int usuarioId)
    {
        if (rol == "Administrador")
        {
            return true;
        }

        if (rol == "Usuario")
        {
            return ticket.UsuarioId == usuarioId;
        }

        if (rol == "Agente")
        {
            return ticket.AgenteId == usuarioId;
        }

        return false;
    }

    // MÉTODO AUXILIAR - CREAR NOTIFICACIÓN
    private void AgregarNotificacion(
        int destinatarioId,
        int autorId,
        int ticketId,
        string mensaje)
    {
        // Evitar que el autor reciba su propia notificación.
        if (destinatarioId == autorId)
        {
            return;
        }

        _context.Notificaciones.Add(new Notificacion
        {
            UsuarioId = destinatarioId,
            TicketId = ticketId,
            Mensaje = mensaje,
            Leida = false,
            Fecha = DateTime.Now
        });
    }

    // MÉTODO AUXILIAR - OBTENER ADMINISTRADORES ACTIVOS
    private async Task<List<int>> ObtenerAdministradoresActivosAsync()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u =>
                u.Rol == "Administrador" &&
                u.Activo)
            .Select(u => u.Id)
            .ToListAsync();
    }

    // MÉTODO AUXILIAR - CREAR NOTIFICACIONES SEGÚN EL ROL
    private async Task NotificarNuevoComentarioAsync(
        Ticket ticket,
        int autorId,
        string? rol,
        string contenido)
    {
        // Evitar notificaciones excesivamente largas.
        var fragmento = contenido.Trim();

        if (fragmento.Length > 120)
        {
            fragmento = fragmento[..120] + "...";
        }

        var mensaje =
            $"Nuevo comentario en el ticket #{ticket.Id}: " +
            $"{ticket.Titulo}. Comentario: {fragmento}";

        if (rol == "Usuario")
        {
            // Notificar al agente asignado.
            if (ticket.AgenteId.HasValue)
            {
                AgregarNotificacion(
                    ticket.AgenteId.Value,
                    autorId,
                    ticket.Id,
                    mensaje);
            }

            // Notificar a los administradores activos.
            var administradores =
                await ObtenerAdministradoresActivosAsync();

            foreach (var administradorId in administradores)
            {
                AgregarNotificacion(
                    administradorId,
                    autorId,
                    ticket.Id,
                    mensaje);
            }
        }
        else if (rol == "Agente")
        {
            // Notificar al usuario propietario del ticket.
            AgregarNotificacion(
                ticket.UsuarioId,
                autorId,
                ticket.Id,
                mensaje);

            // Notificar a los administradores activos.
            var administradores =
                await ObtenerAdministradoresActivosAsync();

            foreach (var administradorId in administradores)
            {
                AgregarNotificacion(
                    administradorId,
                    autorId,
                    ticket.Id,
                    mensaje);
            }
        }
        else if (rol == "Administrador")
        {
            // Notificar al usuario propietario del ticket.
            AgregarNotificacion(
                ticket.UsuarioId,
                autorId,
                ticket.Id,
                mensaje);

            // Notificar al agente asignado, si existe.
            if (ticket.AgenteId.HasValue)
            {
                AgregarNotificacion(
                    ticket.AgenteId.Value,
                    autorId,
                    ticket.Id,
                    mensaje);
            }
        }
    }

    // POST - AGREGAR COMENTARIO
    [HttpPost("{ticketId:int}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> CrearComentario(
        int ticketId,
        [FromBody] CreateComentarioDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                mensaje = "Los datos del comentario son obligatorios."
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(dto.Contenido))
        {
            return BadRequest(new
            {
                mensaje = "El comentario no puede estar vacío."
            });
        }

        if (!TryGetAuthenticatedUserId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje =
                    "No se pudo identificar al usuario autenticado."
            });
        }

        var rol = ObtenerRol();

        if (rol != "Administrador" &&
            rol != "Agente" &&
            rol != "Usuario")
        {
            return Forbid();
        }

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        // Comprobar que el usuario tenga acceso al ticket.
        if (!TienePermisoSobreTicket(ticket, rol, usuarioId))
        {
            return Forbid();
        }

        var contenido = dto.Contenido.Trim();
        var fechaActual = DateTime.Now;

        var comentario = new ComentarioTicket
        {
            TicketId = ticketId,
            UsuarioId = usuarioId,
            Contenido = contenido,
            Fecha = fechaActual
        };

        _context.ComentariosTickets.Add(comentario);

        // Crear las notificaciones según el rol del autor.
        await NotificarNuevoComentarioAsync(
            ticket,
            usuarioId,
            rol,
            contenido);

        // Guardar comentario y notificaciones.
        await _context.SaveChangesAsync();

        var usuarioNombre = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => u.Nombre)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            mensaje = "Comentario agregado correctamente.",
            id = comentario.Id,
            ticketId = comentario.TicketId,
            usuarioId = comentario.UsuarioId,
            usuarioNombre = usuarioNombre ?? "Usuario desconocido",
            contenido = comentario.Contenido,
            fecha = comentario.Fecha
        });
    }

    // GET - OBTENER COMENTARIOS DE UN TICKET
    [HttpGet("{ticketId:int}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> ObtenerComentarios(int ticketId)
    {
        if (!TryGetAuthenticatedUserId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje =
                    "No se pudo identificar al usuario autenticado."
            });
        }

        var rol = ObtenerRol();

        if (rol != "Administrador" &&
            rol != "Agente" &&
            rol != "Usuario")
        {
            return Forbid();
        }

        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        // Comprobar que el usuario tenga acceso al ticket.
        if (!TienePermisoSobreTicket(ticket, rol, usuarioId))
        {
            return Forbid();
        }

        var comentarios = await _context.ComentariosTickets
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Id)
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
