using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Models;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketDeskDbContext _context;
    private readonly TicketStatusService _ticketStatusService;

    public TicketsController(
        TicketDeskDbContext context,
        TicketStatusService ticketStatusService)
    {
        _context = context;
        _ticketStatusService = ticketStatusService;
    }

    // GET - LISTA DE TICKETS
    [HttpGet]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets()
    {
        var consulta = _context.Tickets
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .Include(t => t.Agente)
            .AsQueryable();

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol == "Usuario")
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

            consulta = consulta.Where(t => t.UsuarioId == usuarioId);
        }

        if (rol == "Agente")
        {
            var agenteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(agenteIdClaim))
            {
                agenteIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (!int.TryParse(agenteIdClaim, out var agenteId))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al agente autenticado."
                });
            }

            consulta = consulta.Where(t => t.AgenteId == agenteId);
        }

        var tickets = await consulta
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                Prioridad = t.Prioridad,

                UsuarioId = t.UsuarioId,
                UsuarioNombre = t.Usuario != null
                    ? t.Usuario.Nombre
                    : string.Empty,

                CategoriaId = t.CategoriaId,
                CategoriaNombre = t.Categoria != null
                    ? t.Categoria.Nombre
                    : string.Empty,

                AgenteId = t.AgenteId,
                AgenteNombre = t.Agente != null
                    ? t.Agente.Nombre
                    : string.Empty,

                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion
            })
            .ToListAsync();

        return Ok(tickets);
    }

    // GET - OBTENER UN TICKET
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .Include(t => t.Agente)
            .Where(t => t.Id == id)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                Prioridad = t.Prioridad,

                UsuarioId = t.UsuarioId,
                UsuarioNombre = t.Usuario != null
                    ? t.Usuario.Nombre
                    : string.Empty,

                CategoriaId = t.CategoriaId,
                CategoriaNombre = t.Categoria != null
                    ? t.Categoria.Nombre
                    : string.Empty,

                AgenteId = t.AgenteId,
                AgenteNombre = t.Agente != null
                    ? t.Agente.Nombre
                    : string.Empty,

                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion
            })
            .FirstOrDefaultAsync();

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol == "Usuario")
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

            if (ticket.UsuarioId != usuarioId)
            {
                return Forbid();
            }
        }

        if (rol == "Agente")
        {
            var agenteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(agenteIdClaim))
            {
                agenteIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (!int.TryParse(agenteIdClaim, out var agenteId))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al agente autenticado."
                });
            }

            if (ticket.AgenteId != agenteId)
            {
                return Forbid();
            }
        }

        return Ok(ticket);
    }

    // POST - CREAR TICKET
    [HttpPost]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket(
        CreateTicketDto dto)
    {
        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u =>
                u.Id == dto.UsuarioId &&
                u.Activo);

        if (!usuarioExiste)
        {
            return BadRequest(new
            {
                mensaje = "El usuario seleccionado no existe o está inactivo."
            });
        }

        var categoriaExiste = await _context.Categorias
            .AnyAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Activa);

        if (!categoriaExiste)
        {
            return BadRequest(new
            {
                mensaje = "La categoría seleccionada no existe o está inactiva."
            });
        }

        var ticket = new Ticket
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Estado = "Abierto",
            Prioridad = dto.Prioridad,
            UsuarioId = dto.UsuarioId,
            CategoriaId = dto.CategoriaId,
            FechaCreacion = DateTime.Now
        };

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var historial = new HistorialTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioAutenticadoId,
            Accion = "Ticket creado",
            EstadoAnterior = null,
            EstadoNuevo = ticket.Estado,
            Comentario = "Se creó el ticket.",
            Fecha = DateTime.Now
        };

        _context.HistorialTickets.Add(historial);

        await _context.SaveChangesAsync();

        var ticketCreado = await _context.Tickets
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .Include(t => t.Agente)
            .Where(t => t.Id == ticket.Id)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                Prioridad = t.Prioridad,

                UsuarioId = t.UsuarioId,
                UsuarioNombre = t.Usuario != null
                    ? t.Usuario.Nombre
                    : string.Empty,

                CategoriaId = t.CategoriaId,
                CategoriaNombre = t.Categoria != null
                    ? t.Categoria.Nombre
                    : string.Empty,

                AgenteId = t.AgenteId,
                AgenteNombre = t.Agente != null
                    ? t.Agente.Nombre
                    : string.Empty,

                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetTicket),
            new { id = ticketCreado.Id },
            ticketCreado
        );
    }

    // PUT - ACTUALIZAR TICKET
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Agente")]
    public async Task<IActionResult> UpdateTicket(
        int id,
        [FromBody] UpdateTicketDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (dto == null)
        {
            return BadRequest(new
            {
                mensaje = "Los datos del ticket son obligatorios."
            });
        }

        var ticketExistente = await _context.Tickets
            .FindAsync(id);

        if (ticketExistente == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol == "Agente")
        {
            var agenteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(agenteIdClaim))
            {
                agenteIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (!int.TryParse(agenteIdClaim, out var agenteId))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al agente autenticado."
                });
            }

            if (ticketExistente.AgenteId != agenteId)
            {
                return Forbid();
            }
        }

        if (!_ticketStatusService.EsTransicionValida(
            ticketExistente.Estado,
            dto.Estado))
        {
            return BadRequest(new
            {
                mensaje =
                    $"No se permite cambiar el estado de '{ticketExistente.Estado}' a '{dto.Estado}'."
            });
        }

        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u =>
                u.Id == dto.UsuarioId &&
                u.Activo);

        if (!usuarioExiste)
        {
            return BadRequest(new
            {
                mensaje = "El usuario seleccionado no existe o está inactivo."
            });
        }

        var categoriaExiste = await _context.Categorias
            .AnyAsync(c =>
                c.Id == dto.CategoriaId &&
                c.Activa);

        if (!categoriaExiste)
        {
            return BadRequest(new
            {
                mensaje = "La categoría seleccionada no existe o está inactiva."
            });
        }

        if (dto.AgenteId.HasValue)
        {
            var agenteExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Id == dto.AgenteId.Value &&
                    u.Rol == "Agente" &&
                    u.Activo);

            if (!agenteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El agente seleccionado no existe, está inactivo o no tiene el rol de Agente."
                });
            }
        }

        var estadoAnterior = ticketExistente.Estado;
        var agenteAnterior = ticketExistente.AgenteId;

        ticketExistente.Titulo = dto.Titulo;
        ticketExistente.Descripcion = dto.Descripcion;
        ticketExistente.Estado = dto.Estado;
        ticketExistente.Prioridad = dto.Prioridad;
        ticketExistente.UsuarioId = dto.UsuarioId;
        ticketExistente.CategoriaId = dto.CategoriaId;
        ticketExistente.AgenteId = dto.AgenteId;
        ticketExistente.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        // Registrar cambio de estado y crear notificación
        if (estadoAnterior != ticketExistente.Estado)
        {
            var historialEstado = new HistorialTicket
            {
                TicketId = ticketExistente.Id,
                UsuarioId = usuarioAutenticadoId,
                Accion = "Cambio de estado",
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = ticketExistente.Estado,
                Comentario =
                    $"El estado cambió de '{estadoAnterior}' a '{ticketExistente.Estado}'.",
                Fecha = DateTime.Now
            };

            _context.HistorialTickets.Add(historialEstado);

            // Crear notificación para el usuario propietario del ticket
            var notificacion = new Notificacion
            {
                UsuarioId = ticketExistente.UsuarioId,
                TicketId = ticketExistente.Id,
                Mensaje =
                    $"Tu ticket #{ticketExistente.Id}: {ticketExistente.Titulo} ha sido marcado como {ticketExistente.Estado}.",
                Leida = false,
                Fecha = DateTime.Now
            };

            _context.Notificaciones.Add(notificacion);
        }

        // Registrar cambio de agente
        if (agenteAnterior != ticketExistente.AgenteId)
        {
            string comentarioAgente;

            if (ticketExistente.AgenteId.HasValue)
            {
                var nuevoAgente = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Id == ticketExistente.AgenteId.Value);

                comentarioAgente = nuevoAgente != null
                    ? $"Ticket asignado al agente {nuevoAgente.Nombre}."
                    : $"Ticket asignado al agente ID {ticketExistente.AgenteId}.";
            }
            else
            {
                comentarioAgente = "Se quitó la asignación del agente.";
            }

            var historialAsignacion = new HistorialTicket
            {
                TicketId = ticketExistente.Id,
                UsuarioId = usuarioAutenticadoId,
                Accion = "Cambio de agente",
                EstadoAnterior = null,
                EstadoNuevo = null,
                Comentario = comentarioAgente,
                Fecha = DateTime.Now
            };

            _context.HistorialTickets.Add(historialAsignacion);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - ELIMINAR TICKET
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets
            .FindAsync(id);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        _context.Tickets.Remove(ticket);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT - ASIGNAR TICKET A UN AGENTE
    [HttpPut("{id}/asignar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AsignarTicket(
        int id,
        [FromBody] AsignarTicketDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ticket = await _context.Tickets
            .FindAsync(id);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        var agente = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Id == dto.AgenteId &&
                u.Rol == "Agente" &&
                u.Activo);

        if (agente == null)
        {
            return BadRequest(new
            {
                mensaje = "El agente seleccionado no existe, está inactivo o no tiene el rol de Agente."
            });
        }

        // Evitar asignaciones duplicadas
        if (ticket.AgenteId == dto.AgenteId)
        {
            return BadRequest(new
            {
                mensaje = "El ticket ya está asignado a este agente."
            });
        }

        ticket.AgenteId = dto.AgenteId;
        ticket.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(usuarioIdClaim))
        {
            usuarioIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        if (!int.TryParse(usuarioIdClaim, out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        // Registrar la asignación en el historial
        var historial = new HistorialTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioAutenticadoId,
            Accion = "Ticket asignado",
            EstadoAnterior = null,
            EstadoNuevo = null,
            Comentario = $"El ticket fue asignado al agente {agente.Nombre}.",
            Fecha = DateTime.Now
        };

        _context.HistorialTickets.Add(historial);

        // Crear notificación para el agente
        var notificacion = new Notificacion
        {
            UsuarioId = agente.Id,
            TicketId = ticket.Id,
            Mensaje = $"Se te ha asignado el ticket #{ticket.Id}: {ticket.Titulo}.",
            Leida = false,
            Fecha = DateTime.Now
        };

        _context.Notificaciones.Add(notificacion);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Ticket asignado correctamente.",
            ticketId = ticket.Id,
            agenteId = ticket.AgenteId,
            agenteNombre = agente.Nombre
        });
    }
}

