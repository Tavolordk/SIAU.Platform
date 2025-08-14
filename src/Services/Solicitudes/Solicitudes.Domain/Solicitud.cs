using SharedKernel.Domain.Enums;

namespace Solicitudes.Domain;

public sealed class Solicitud
{
	public uint Id { get; init; }
	public string Folio { get; private set; } = "";
	public uint PersonaId { get; private set; }
	public EstadoSolicitud Estado { get; private set; } = EstadoSolicitud.Creada;
	public string NumeroOficio { get; private set; } = "";
	public DateOnly FechaSolicitud { get; private set; }

	private Solicitud() { }
	public static Solicitud Crear(uint personaId, EstadoSolicitud estado, string oficio, DateOnly fecha) =>
		new() { PersonaId = personaId, Estado = estado, NumeroOficio = oficio, FechaSolicitud = fecha };
}
