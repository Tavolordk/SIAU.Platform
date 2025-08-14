using SharedKernel.Domain.Enums;
using SharedKernel.Results;

namespace Solicitudes.Application.CambiarEstado;

public sealed record CambiarEstadoCommand(uint SolicitudId, EstadoSolicitud Estado, uint UsuarioId, string Comentario);
public interface ICambiarEstadoHandler
{
	Task<Result<bool>> Handle(CambiarEstadoCommand cmd, CancellationToken ct);
}
