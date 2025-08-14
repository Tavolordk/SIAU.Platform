using MySql.Data.MySqlClient;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using Solicitudes.Application.CambiarEstado;

namespace Solicitudes.Infrastructure;

public sealed class CambiarEstadoHandler(IStoredProcExecutor spx) : ICambiarEstadoHandler
{
	public async Task<Result<bool>> Handle(CambiarEstadoCommand cmd, CancellationToken ct)
	{
		await spx.ExecAsync("sp_cambiar_estado_solicitud", command =>
		{
			var c = (MySqlCommand)command;
			c.Parameters.AddWithValue("@p_solicitud_id", cmd.SolicitudId);
			c.Parameters.AddWithValue("@p_estado_clave", cmd.Estado.ToString());
			c.Parameters.AddWithValue("@p_usuario_id", cmd.UsuarioId);
			c.Parameters.AddWithValue("@p_comentario", cmd.Comentario);
		}, ct);
		return Result<bool>.Success(true);
	}
}
