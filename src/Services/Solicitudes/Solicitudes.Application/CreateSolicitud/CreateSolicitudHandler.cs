using SharedKernel.Data;
using SharedKernel.Results;
using System.Data;

namespace Solicitudes.Application.CreateSolicitud;

public sealed class CreateSolicitudHandler(IStoredProcExecutor spx) : ICreateSolicitudHandler
{
	public async Task<Result<(uint solicitudId, string folio)>> Handle(CreateSolicitudCommand cmd, CancellationToken ct)
	{
		// Parámetros de entrada
		var pPersonaId = spx.CreateParameter("@p_persona_id", cmd.PersonaId, DbType.UInt32);
		var pEstadoClave = spx.CreateParameter("@p_estado_clave", cmd.Estado.ToString(), DbType.String);
		var pNumeroOficio = spx.CreateParameter("@p_numero_oficio", cmd.NumeroOficio, DbType.String);
		pNumeroOficio.Size = 50;
		var pFechaSol = spx.CreateParameter("@p_fecha_solicitud",
												cmd.FechaSolicitud.ToDateTime(TimeOnly.MinValue),
												DbType.DateTime);
		var pUsuarioId = spx.CreateParameter("@p_usuario_id", cmd.UsuarioId, DbType.UInt32);

		// Parámetros de salida
		var pOutId = spx.CreateParameter("@p_solicitud_id", null, DbType.UInt32, ParameterDirection.Output);
		var pOutFolio = spx.CreateParameter("@p_folio", null, DbType.String, ParameterDirection.Output);
		pOutFolio.Size = 30; // importante para strings de salida en MySQL

		// Ejecuta SP (misma instancia de parámetros para leer los outputs después)
		await spx.ExecuteAsync("sp_crear_solicitud",
			new IDbDataParameter[] { pPersonaId, pEstadoClave, pNumeroOficio, pFechaSol, pUsuarioId, pOutId, pOutFolio },
			ct);

		// Recupera salidas
		var outId = Convert.ToUInt32(pOutId.Value is DBNull ? 0 : pOutId.Value);
		var outFolio = Convert.ToString(pOutFolio.Value) ?? string.Empty;

		return Result<(uint solicitudId, string folio)>.Success((outId, outFolio));
	}
}
