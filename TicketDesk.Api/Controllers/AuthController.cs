using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TicketDeskDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthController(
        TicketDeskDbContext context,
        PasswordService passwordService,
        JwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // ========================================
        // BUSCAR USUARIO
        // ========================================

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        // ========================================
        // COMPROBAR SI ESTÁ ACTIVO
        // ========================================

        if (!usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "El usuario está inactivo."
            });
        }

        // ========================================
        // VERIFICAR CONTRASEÑA
        // ========================================

        var passwordCorrecta = _passwordService.VerifyPassword(
            dto.Password,
            usuario.PasswordHash
        );

        if (!passwordCorrecta)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        // ========================================
        // GENERAR JWT
        // ========================================

        var token = _jwtService.GenerateToken(usuario);

        // ========================================
        // LOGIN EXITOSO
        // ========================================

        return Ok(new
        {
            mensaje = "Inicio de sesión exitoso.",

            token = token,

            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                rol = usuario.Rol
            }
        });
    }
}