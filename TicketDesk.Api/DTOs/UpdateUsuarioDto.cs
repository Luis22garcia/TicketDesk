using System.ComponentModel.DataAnnotations;

namespace TicketDesk.Api.DTOs;

public class UpdateUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression(
        "^(Administrador|Agente|Usuario)$",
        ErrorMessage = "El rol debe ser Administrador, Agente o Usuario."
    )]
    public string Rol { get; set; } = "Usuario";

    public bool Activo { get; set; } = true;
}