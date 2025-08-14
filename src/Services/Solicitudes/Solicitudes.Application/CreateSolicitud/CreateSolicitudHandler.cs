using System.Data;
using SharedKernel.Data;
using SharedKernel.Results;

namespace Solicitudes.Application.CreateSolicitud;

public sealed class CreateSolicitudHandler(IStoredProcExecutor spx) : ICreateSolicitudHandler
{
	public async Task<Result<(uint solicitudId, string folio)>> Handle(CreateSolicitudCommand cmd, CancellationToken ct)
	{
		static void Add(IDbCommand db, string name, object? value, DbType? type = null, int? size = null,
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

		var res = await spx.ExecAsync("sp_crear_solicitud", db =>
		{
			db.CommandType = CommandType.StoredProcedure;

			Add(db, "@p_persona_id", cmd.PersonaId, DbType.UInt32);
			Add(db, "@p_estado_clave", cmd.Estado.ToString(), DbType.String, 20);
			Add(db, "@p_numero_oficio", cmd.NumeroOficio, DbType.String, 50);
			Add(db, "@p_fecha_solicitud", cmd.FechaSolicitud.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
			Add(db, "@p_usuario_id", cmd.UsuarioId, DbType.UInt32);

			// outputs
			Add(db, "@p_solicitud_id", null, DbType.UInt32, direction: ParameterDirection.Output);
			Add(db, "@p_folio", null, DbType.String, size: 30, direction: ParameterDirection.Output);
		}, ct);

		var outId = Convert.ToUInt32(res.Outputs.GetValueOrDefault("@p_solicitud_id") ?? 0);
		var outFolio = Convert.ToString(res.Outputs.GetValueOrDefault("@p_folio")) ?? string.Empty;

		return Result<(uint, string)>.Success((outId, outFolio));
	}
}
