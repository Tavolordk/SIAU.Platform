using SharedKernel.Domain.Enums;
using SharedKernel.Results;

namespace Solicitudes.Application.CreateSolicitud;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public sealed record CreateSolicitudCommand(
	uint PersonaId,
	EstadoSolicitud Estado,
	string NumeroOficio,
	DateOnly FechaSolicitud,
	uint UsuarioId
);
public interface ICreateSolicitudHandler
{
	Task<Result<(uint solicitudId, string folio)>> Handle(CreateSolicitudCommand cmd, CancellationToken ct);
}
