using System.ComponentModel.DataAnnotations;

namespace TicketDesk.Api.DTOs;

public class AsignarTicketDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un agente válido.")]
    public int AgenteId { get; set; }
}