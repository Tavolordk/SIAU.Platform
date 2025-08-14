using SharedKernel.Data;                 // IStoredProcExecutor
using SharedKernel.Domain.Enums;
using SharedKernel.Results;
using Solicitudes.Application.CambiarEstado;
using System.Data;

namespace Solicitudes.Infrastructure;

public sealed class CambiarEstadoHandler(IStoredProcExecutor spx) : ICambiarEstadoHandler
{
	public async Task<Result<bool>> Handle(CambiarEstadoCommand cmd, CancellationToken ct)
	{
		// Construye parámetros (puedes pasar nombres con o sin @; el executor los normaliza si así lo implementaste)
		var pSolicitudId = spx.CreateParameter("@p_solicitud_id", cmd.SolicitudId, DbType.UInt32);

		var pEstadoClave = spx.CreateParameter("@p_estado_clave", cmd.Estado.ToString(), DbType.String);
		pEstadoClave.Size = 20;

		var pUsuarioId = spx.CreateParameter("@p_usuario_id", cmd.UsuarioId, DbType.UInt32);

		var pComentario = spx.CreateParameter("@p_comentario", cmd.Comentario, DbType.String);
		pComentario.Size = 500;

		await spx.ExecuteAsync(
			StoredProcedures.CambiarEstadoSolicitud,
			new IDbDataParameter[] { pSolicitudId, pEstadoClave, pUsuarioId, pComentario },
			ct);

		return Result<bool>.Success(true);
	}
}
