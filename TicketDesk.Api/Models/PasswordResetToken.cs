namespace TicketDesk.Api.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; } = false;

    public DateTime FechaCreacion { get; set; }

    public Usuario? Usuario { get; set; }
}