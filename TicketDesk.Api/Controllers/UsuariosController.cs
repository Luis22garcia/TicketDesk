using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly TicketDeskDbContext _context;
    private readonly PasswordService _passwordService;

    public UsuariosController(
        TicketDeskDbContext context,
        PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    // ========================================
    // GET: api/usuarios
    // Solo Administrador
    // ========================================

    [HttpGet]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Email,
                u.Rol,
                u.Activo,
                u.FechaCreacion
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // ========================================
    // GET: api/usuarios/1
    // Solo Administrador
    // ========================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Email,
                u.Rol,
                u.Activo,
                u.FechaCreacion
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        return Ok(usuario);
    }

    // ========================================
    // POST: api/usuarios
    // Solo Administrador
    // ========================================

    [HttpPost]
    public async Task<IActionResult> CreateUsuario(CreateUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un usuario con ese correo."
            });
        }

        var usuario = new Models.Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = _passwordService.HashPassword(dto.Password),
            Rol = dto.Rol,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUsuario),
            new { id = usuario.Id },
            new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol,
                usuario.Activo,
                usuario.FechaCreacion
            }
        );
    }

    // ========================================
    // PUT: api/usuarios/1
    // Solo Administrador
    // ========================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUsuario(
        int id,
        UpdateUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _context.Usuarios
            .FindAsync(id);

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u =>
                u.Email == dto.Email &&
                u.Id != id);

        if (emailExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe otro usuario con ese correo."
            });
        }

        usuario.Nombre = dto.Nombre;
        usuario.Email = dto.Email;
        usuario.Rol = dto.Rol;
        usuario.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ========================================
    // DELETE: api/usuarios/1
    // Solo Administrador
    //
    // No eliminamos físicamente.
    // Desactivamos el usuario.
    // ========================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios
            .FindAsync(id);

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        usuario.Activo = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}