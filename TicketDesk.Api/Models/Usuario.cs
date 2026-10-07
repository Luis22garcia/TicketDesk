namespace TicketDesk.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Rol { get; set; } = "Usuario";

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}