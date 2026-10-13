
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Models;
using TicketDesk.Api.Services;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TicketDeskDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        TicketDeskDbContext context,
        PasswordService passwordService,
        JwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        if (!usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "El usuario está inactivo."
            });
        }

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

        var token = _jwtService.GenerateToken(usuario);

        return Ok(new
        {
            mensaje = "Inicio de sesión exitoso.",
            token,
            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                rol = usuario.Rol
            }
        });
    }

    // POST: api/auth/solicitar-recuperacion
    [HttpPost("solicitar-recuperacion")]
    public async Task<IActionResult> SolicitarRecuperacion(
        SolicitarRecuperacionDto dto,
        CancellationToken cancellationToken)
    {
        var correo = dto.Email.Trim();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Email == correo,
                cancellationToken
            );

        // No revelar si el correo está registrado.
        if (usuario == null || !usuario.Activo)
        {
            return Ok(new
            {
                mensaje = "Si el correo está registrado y activo, recibirás instrucciones para recuperar tu contraseña."
            });
        }

        // Invalidar tokens anteriores todavía no utilizados.
        var tokensAnteriores = await _context.PasswordResetTokens
            .Where(t => t.UsuarioId == usuario.Id && !t.Usado)
            .ToListAsync(cancellationToken);

        foreach (var tokenAnterior in tokensAnteriores)
        {
            tokenAnterior.Usado = true;
        }

        // Crear token criptográficamente seguro.
        var tokenPlano = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32)
        )
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

        // Guardar únicamente el hash del token.
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(tokenPlano))
        );

        var resetToken = new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenHash,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddMinutes(30),
            Usado = false
        };

        _context.PasswordResetTokens.Add(resetToken);

        await _context.SaveChangesAsync(cancellationToken);

        // La dirección del frontend se configura en appsettings.json.
        var frontendUrl = _configuration["App:FrontendUrl"];

        if (string.IsNullOrWhiteSpace(frontendUrl) ||
            !Uri.TryCreate(frontendUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != Uri.UriSchemeHttp))
        {
            _logger.LogError(
                "La configuración App:FrontendUrl no es válida."
            );

            return StatusCode(500, new
            {
                mensaje = "No se pudo procesar la solicitud de recuperación."
            });
        }

        var enlace = $"{frontendUrl.TrimEnd('/')}/restablecer-password?token={Uri.EscapeDataString(tokenPlano)}";

        var mensajeHtml = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;">
                <h2>Recuperación de contraseña</h2>
                <p>Hola {System.Net.WebUtility.HtmlEncode(usuario.Nombre)},</p>
                <p>Recibimos una solicitud para cambiar la contraseña de tu cuenta de TicketDesk 2.0.</p>
                <p>
                    <a href="{System.Net.WebUtility.HtmlEncode(enlace)}"
                       style="display:inline-block;padding:12px 20px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:6px;">
                        Restablecer contraseña
                    </a>
                </p>
                <p>Este enlace caduca en 30 minutos y solo se puede utilizar una vez.</p>
                <p>Si no solicitaste el cambio, puedes ignorar este correo.</p>
            </div>
            """;

        try
        {
            await _emailService.EnviarAsync(
                usuario.Email,
                "Recuperación de contraseña - TicketDesk 2.0",
                mensajeHtml,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo enviar el correo de recuperación."
            );

            return StatusCode(500, new
            {
                mensaje = "No se pudo enviar el correo de recuperación. Inténtalo más tarde."
            });
        }

        return Ok(new
        {
            mensaje = "Si el correo está registrado y activo, recibirás instrucciones para recuperar tu contraseña."
        });
    }

    // POST: api/auth/restablecer-password
    [HttpPost("restablecer-password")]
    public async Task<IActionResult> RestablecerPassword(
        RestablecerPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(dto.Token))
        );

        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash,
                cancellationToken
            );

        if (resetToken == null ||
            resetToken.Usado ||
            resetToken.FechaExpiracion <= DateTime.UtcNow ||
            resetToken.Usuario == null ||
            !resetToken.Usuario.Activo)
        {
            return BadRequest(new
            {
                mensaje = "El enlace de recuperación es inválido, ha caducado o ya fue utilizado."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.NuevaPassword) ||
            dto.NuevaPassword.Length < 8)
        {
            return BadRequest(new
            {
                mensaje = "La nueva contraseña debe tener al menos 8 caracteres."
            });
        }

        resetToken.Usuario.PasswordHash =
            _passwordService.HashPassword(dto.NuevaPassword);

        resetToken.Usado = true;

        // Invalidar cualquier otro token pendiente del mismo usuario.
        var otrosTokens = await _context.PasswordResetTokens
            .Where(t =>
                t.UsuarioId == resetToken.UsuarioId &&
                t.Id != resetToken.Id &&
                !t.Usado)
            .ToListAsync(cancellationToken);

        foreach (var otroToken in otrosTokens)
        {
            otroToken.Usado = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            mensaje = "Tu contraseña se restableció correctamente. Ya puedes iniciar sesión."
        });
    }
}

// DTO para solicitar recuperación.
public class SolicitarRecuperacionDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

// DTO para restablecer contraseña.
public class RestablecerPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NuevaPassword { get; set; } = string.Empty;
}
