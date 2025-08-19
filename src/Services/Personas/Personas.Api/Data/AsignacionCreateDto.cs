using System.ComponentModel.DataAnnotations;

public sealed record AsignacionCreateDto
{
	[Required] public string TipoAsignacion { get; init; } = default!;
	[Required] public uint EstructuraId { get; init; }
	[Required] public string Cargo { get; init; } = default!;
	public string? Funciones { get; init; }
	public string? NumeroEmpleado { get; init; }
	[Required] public DateOnly FechaInicio { get; init; }
}
