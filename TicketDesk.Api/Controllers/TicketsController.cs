
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

    // MÉTODO AUXILIAR - OBTENER EL ID DEL USUARIO AUTENTICADO
    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            userIdClaim =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        return int.TryParse(userIdClaim, out userId);
    }

    // GET - LISTAR TICKETS
    [HttpGet]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets()
    {
        var consulta = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol == "Usuario")
        {
            if (!TryGetAuthenticatedUserId(out var usuarioId))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            consulta = consulta.Where(t => t.UsuarioId == usuarioId);
        }
        else if (rol == "Agente")
        {
            if (!TryGetAuthenticatedUserId(out var agenteId))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al agente autenticado."
                });
            }

            consulta = consulta.Where(t => t.AgenteId == agenteId);
        }

        var tickets = await consulta
            .OrderByDescending(t => t.FechaCreacion)
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

    // GET - CONSULTAR LA CARGA DE TRABAJO DE LOS AGENTES
    // Solo el Administrador puede consultar esta información.
    [HttpGet("agentes/carga")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerCargaAgentes()
    {
        var agentes = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == "Agente" && u.Activo)
            .Select(u => new
            {
                agenteId = u.Id,
                agenteNombre = u.Nombre,

                totalTickets = _context.Tickets.Count(t =>
                    t.AgenteId == u.Id),

                ticketsPendientes = _context.Tickets.Count(t =>
                    t.AgenteId == u.Id &&
                    t.Estado != "Resuelto" &&
                    t.Estado != "Cerrado"),

                ticketsAbiertos = _context.Tickets.Count(t =>
                    t.AgenteId == u.Id &&
                    t.Estado == "Abierto"),

                ticketsEnProceso = _context.Tickets.Count(t =>
                    t.AgenteId == u.Id &&
                    t.Estado == "En proceso")
            })
            .OrderBy(a => a.ticketsPendientes)
            .ThenBy(a => a.totalTickets)
            .ToListAsync();

        return Ok(agentes);
    }

    // GET - OBTENER UN TICKET POR ID
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(int id)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
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
            if (!TryGetAuthenticatedUserId(out var usuarioId))
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
        else if (rol == "Agente")
        {
            if (!TryGetAuthenticatedUserId(out var agenteId))
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
        [FromBody] CreateTicketDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                mensaje = "Los datos del ticket son obligatorios."
            });
        }

        if (!TryGetAuthenticatedUserId(out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        // DETERMINAR EL PROPIETARIO DEL TICKET.
        int usuarioIdTicket;

        if (rol == "Usuario")
        {
            // El Usuario siempre crea tickets a su propio nombre.
            // Su ID se obtiene del token JWT, no del frontend.
            usuarioIdTicket = usuarioAutenticadoId;
        }
        else
        {
            // Administrador y Agente deben seleccionar un usuario válido.
            if (!dto.UsuarioId.HasValue || dto.UsuarioId.Value < 1)
            {
                return BadRequest(new
                {
                    mensaje = "Debes seleccionar un usuario válido."
                });
            }

            usuarioIdTicket = dto.UsuarioId.Value;
        }

        // VERIFICAR QUE EL USUARIO EXISTA Y ESTÉ ACTIVO.
        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u =>
                u.Id == usuarioIdTicket &&
                u.Activo);

        if (!usuarioExiste)
        {
            return BadRequest(new
            {
                mensaje = "El usuario seleccionado no existe o está inactivo."
            });
        }

        // VERIFICAR QUE LA CATEGORÍA EXISTA Y ESTÉ ACTIVA.
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

        // CREAR EL TICKET.
        var ticket = new Ticket
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Estado = "Abierto",
            Prioridad = dto.Prioridad,
            UsuarioId = usuarioIdTicket,
            CategoriaId = dto.CategoriaId,
            FechaCreacion = DateTime.Now
        };

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        // REGISTRAR EL HISTORIAL DE CREACIÓN.
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

        // OBTENER EL NOMBRE DEL USUARIO QUE REPORTÓ EL PROBLEMA.
        var usuarioNombre = await _context.Usuarios
            .Where(u => u.Id == ticket.UsuarioId)
            .Select(u => u.Nombre)
            .FirstOrDefaultAsync() ?? "Usuario";

        var categoriaNombre = await _context.Categorias
            .Where(c => c.Id == ticket.CategoriaId)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync() ?? "Sin categoría";

        // NOTIFICAR A TODOS LOS ADMINISTRADORES ACTIVOS.
        var administradoresCreacion = await _context.Usuarios
            .Where(u => u.Rol == "Administrador" && u.Activo)
            .Select(u => u.Id)
            .ToListAsync();

        var descripcion = string.IsNullOrWhiteSpace(ticket.Descripcion)
            ? "Sin descripción."
            : ticket.Descripcion.Trim();

        foreach (var administradorId in administradoresCreacion)
        {
            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = administradorId,
                TicketId = ticket.Id,
                Mensaje =
                    $"Nuevo ticket #{ticket.Id}: {ticket.Titulo}. " +
                    $"Reportado por: {usuarioNombre}. " +
                    $"Descripción: {descripcion}. " +
                    $"Prioridad: {ticket.Prioridad}. " +
                    $"Categoría: {categoriaNombre}.",
                Leida = false,
                Fecha = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();

        // DEVOLVER EL TICKET CREADO.
        var ticketCreado = await _context.Tickets
            .AsNoTracking()
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
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Agente")]
    public async Task<IActionResult> UpdateTicket(
        int id,
        [FromBody] UpdateTicketDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                mensaje = "Los datos del ticket son obligatorios."
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryGetAuthenticatedUserId(out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario autenticado."
            });
        }

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                mensaje = "El ticket no existe."
            });
        }

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        // UN AGENTE SOLO PUEDE ACTUALIZAR TICKETS ASIGNADOS A ÉL.
        if (rol == "Agente" && ticket.AgenteId != usuarioAutenticadoId)
        {
            return Forbid();
        }

        if (!_ticketStatusService.EsTransicionValida(
            ticket.Estado,
            dto.Estado))
        {
            return BadRequest(new
            {
                mensaje =
                    $"No se permite cambiar el estado de '{ticket.Estado}' a '{dto.Estado}'."
            });
        }

        // SOLO EL ADMINISTRADOR PUEDE MODIFICAR PROPIETARIO,
        // CATEGORÍA Y AGENTE.
        if (rol == "Administrador")
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
        }

        var estadoAnterior = ticket.Estado;
        var agenteAnterior = ticket.AgenteId;

        ticket.Titulo = dto.Titulo;
        ticket.Descripcion = dto.Descripcion;
        ticket.Estado = dto.Estado;
        ticket.Prioridad = dto.Prioridad;
        ticket.FechaActualizacion = DateTime.Now;

        if (rol == "Administrador")
        {
            ticket.UsuarioId = dto.UsuarioId;
            ticket.CategoriaId = dto.CategoriaId;
            ticket.AgenteId = dto.AgenteId;
        }

        // NOTIFICAR CUANDO CAMBIA EL ESTADO.
        if (estadoAnterior != ticket.Estado)
        {
            _context.HistorialTickets.Add(new HistorialTicket
            {
                TicketId = ticket.Id,
                UsuarioId = usuarioAutenticadoId,
                Accion = "Cambio de estado",
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = ticket.Estado,
                Comentario =
                    $"El estado cambió de '{estadoAnterior}' a '{ticket.Estado}'.",
                Fecha = DateTime.Now
            });

            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = ticket.UsuarioId,
                TicketId = ticket.Id,
                Mensaje =
                    $"Tu ticket #{ticket.Id}: {ticket.Titulo} " +
                    $"ha cambiado al estado {ticket.Estado}.",
                Leida = false,
                Fecha = DateTime.Now
            });

            var administradoresEstado = await _context.Usuarios
                .Where(u =>
                    u.Rol == "Administrador" &&
                    u.Activo &&
                    u.Id != ticket.UsuarioId)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var administradorId in administradoresEstado)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = administradorId,
                    TicketId = ticket.Id,
                    Mensaje =
                        $"El ticket #{ticket.Id}: {ticket.Titulo} " +
                        $"cambió al estado {ticket.Estado}.",
                    Leida = false,
                    Fecha = DateTime.Now
                });
            }
        }

        // NOTIFICAR CUANDO CAMBIA LA ASIGNACIÓN DEL AGENTE.
        if (rol == "Administrador" &&
            agenteAnterior != ticket.AgenteId)
        {
            string comentarioAgente;
            string nombreNuevoAgente = string.Empty;

            if (ticket.AgenteId.HasValue)
            {
                nombreNuevoAgente = await _context.Usuarios
                    .Where(u => u.Id == ticket.AgenteId.Value)
                    .Select(u => u.Nombre)
                    .FirstOrDefaultAsync() ?? "Agente";

                comentarioAgente =
                    $"Ticket asignado al agente {nombreNuevoAgente}.";
            }
            else
            {
                comentarioAgente = "Se quitó la asignación del agente.";
            }

            _context.HistorialTickets.Add(new HistorialTicket
            {
                TicketId = ticket.Id,
                UsuarioId = usuarioAutenticadoId,
                Accion = "Cambio de agente",
                EstadoAnterior = null,
                EstadoNuevo = null,
                Comentario = comentarioAgente,
                Fecha = DateTime.Now
            });

            if (ticket.AgenteId.HasValue)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = ticket.AgenteId.Value,
                    TicketId = ticket.Id,
                    Mensaje =
                        $"Se te ha asignado el ticket #{ticket.Id}: " +
                        $"{ticket.Titulo}. " +
                        $"Descripción: {ticket.Descripcion}. " +
                        $"Prioridad: {ticket.Prioridad}.",
                    Leida = false,
                    Fecha = DateTime.Now
                });
            }

            var administradoresCambioAsignacion = await _context.Usuarios
                .Where(u => u.Rol == "Administrador" && u.Activo)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var administradorId in administradoresCambioAsignacion)
            {
                var mensajeAsignacion = ticket.AgenteId.HasValue
                    ? $"El ticket #{ticket.Id}: {ticket.Titulo} " +
                      $"fue asignado al agente {nombreNuevoAgente}."
                    : $"Se quitó la asignación de agente del ticket " +
                      $"#{ticket.Id}: {ticket.Titulo}.";

                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = administradorId,
                    TicketId = ticket.Id,
                    Mensaje = mensajeAsignacion,
                    Leida = false,
                    Fecha = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - ELIMINAR TICKET
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

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
    [HttpPut("{id:int}/asignar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AsignarTicket(
        int id,
        [FromBody] AsignarTicketDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                mensaje = "Los datos de asignación son obligatorios."
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryGetAuthenticatedUserId(out var usuarioAutenticadoId))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al administrador autenticado."
            });
        }

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

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

        if (ticket.AgenteId == dto.AgenteId)
        {
            return BadRequest(new
            {
                mensaje = "El ticket ya está asignado a este agente."
            });
        }

        ticket.AgenteId = dto.AgenteId;
        ticket.FechaActualizacion = DateTime.Now;

        _context.HistorialTickets.Add(new HistorialTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioAutenticadoId,
            Accion = "Ticket asignado",
            EstadoAnterior = null,
            EstadoNuevo = null,
            Comentario =
                $"El ticket fue asignado al agente {agente.Nombre}.",
            Fecha = DateTime.Now
        });

        // NOTIFICACIÓN AL AGENTE CON LA DESCRIPCIÓN DEL PROBLEMA.
        _context.Notificaciones.Add(new Notificacion
        {
            UsuarioId = agente.Id,
            TicketId = ticket.Id,
            Mensaje =
                $"Se te ha asignado el ticket #{ticket.Id}: " +
                $"{ticket.Titulo}. " +
                $"Descripción: {ticket.Descripcion}. " +
                $"Prioridad: {ticket.Prioridad}.",
            Leida = false,
            Fecha = DateTime.Now
        });

        // NOTIFICAR A TODOS LOS ADMINISTRADORES ACTIVOS.
        var administradoresAsignacion = await _context.Usuarios
            .Where(u => u.Rol == "Administrador" && u.Activo)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var administradorId in administradoresAsignacion)
        {
            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = administradorId,
                TicketId = ticket.Id,
                Mensaje =
                    $"El ticket #{ticket.Id}: {ticket.Titulo} " +
                    $"fue asignado al agente {agente.Nombre}.",
                Leida = false,
                Fecha = DateTime.Now
            });
        }

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
