using SharedKernel.Data;
using SharedKernel.Domain.Enums;
using SharedKernel.Results;
using Solicitudes.Application.CambiarEstado;
using System.Data;

namespace Solicitudes.Infrastructure;

public sealed class CambiarEstadoHandler(IStoredProcExecutor spx) : ICambiarEstadoHandler
{
	public async Task<Result<bool>> Handle(CambiarEstadoCommand cmd, CancellationToken ct)
	{
		// Helper local para crear parámetros sin depender del provider
		static void Add(IDbCommand db, string name, object? value,
						DbType? type = null, int? size = null,
						ParameterDirection direction = ParameterDirection.Input)
		{
			var p = db.CreateParameter();
			p.ParameterName = name;
			p.Direction = direction;
			if (type.HasValue) p.DbType = type.Value;
			if (size.HasValue) p.Size = size.Value;
			p.Value = value ?? DBNull.Value;
			db.Parameters.Add(p);
		}

		await spx.ExecAsync(StoredProcedures.CambiarEstadoSolicitud, db =>
		{
			db.CommandType = CommandType.StoredProcedure;

			Add(db, "@p_solicitud_id", cmd.SolicitudId, DbType.UInt32);
			Add(db, "@p_estado_clave", cmd.Estado.ToString(), DbType.String, 20);
			Add(db, "@p_usuario_id", cmd.UsuarioId, DbType.UInt32);
			Add(db, "@p_comentario", cmd.Comentario, DbType.String, 500);
		}, ct);

		return Result<bool>.Success(true);
	}
}
