using System.ComponentModel.DataAnnotations;

namespace TicketDesk.Api.DTOs;

public class CreateComentarioDto
{
    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(
        1000,
        MinimumLength = 1,
        ErrorMessage = "El comentario debe tener entre 1 y 1000 caracteres."
    )]
    public string Contenido { get; set; } = string.Empty;
}