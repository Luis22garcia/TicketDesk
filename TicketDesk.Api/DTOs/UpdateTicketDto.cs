using System.ComponentModel.DataAnnotations;

namespace TicketDesk.Api.DTOs;

public class UpdateTicketDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression(
        "^(Abierto|En proceso|Resuelto|Cerrado)$",
        ErrorMessage = "El estado debe ser Abierto, En proceso, Resuelto o Cerrado."
    )]
    public string Estado { get; set; } = "Abierto";

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [RegularExpression(
        "^(Baja|Media|Alta)$",
        ErrorMessage = "La prioridad debe ser Baja, Media o Alta."
    )]
    public string Prioridad { get; set; } = "Media";

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un usuario válido.")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoría válida.")]
    public int CategoriaId { get; set; }
    public int? AgenteId { get; set; }
}