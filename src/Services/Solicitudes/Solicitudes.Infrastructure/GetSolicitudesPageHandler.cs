using SharedKernel.Abstractions;
using SharedKernel.Results;
using Solicitudes.Application.GetSolicitudes;

namespace Solicitudes.Infrastructure;

public sealed class GetSolicitudesPageHandler(IQueryExecutor qx) : IGetSolicitudesPageHandler
{
	public async Task<Result<PageResult<SolicitudListItem>>> Handle(GetSolicitudesPageQuery q, CancellationToken ct)
	{
		var page = Math.Max(1, q.Page);
		var size = Math.Clamp(q.PageSize, 1, 100);
		var offset = (page - 1) * size;

		var total = await qx.ExecuteCountAsync("SELECT COUNT(1) FROM solicitudes", new Dictionary<string, object?>(), ct);

		const string Sql = @"
        SELECT
          s.id,
          s.folio,
          CONCAT(p.nombres,' ',p.primer_apellido,' ',COALESCE(p.segundo_apellido,'')) AS persona_nombre,
          est.clave AS estado_clave,
          s.fecha_solicitud,
          s.numero_oficio,
          s.created_at
        FROM solicitudes s
        JOIN persona p              ON p.id = s.persona_id
        JOIN cat_estado_solicitudes est ON est.id = s.estado_id
        ORDER BY s.created_at DESC
        LIMIT @limit OFFSET @offset";

		var rows = await qx.QueryAsync(Sql, new Dictionary<string, object?>
		{
			["@limit"] = size,
			["@offset"] = offset
		}, ct);

		var items = rows.Select(r => new SolicitudListItem(
			Convert.ToUInt32(r["id"]!),
			Convert.ToString(r["folio"])!,
			Convert.ToString(r["persona_nombre"])!,
			Convert.ToString(r["estado_clave"])!,
			DateOnly.FromDateTime(Convert.ToDateTime(r["fecha_solicitud"]!)),
			Convert.ToString(r["numero_oficio"])!,
			Convert.ToDateTime(r["created_at"]!)
		)).ToList();

		return Result<PageResult<SolicitudListItem>>.Success(
			new PageResult<SolicitudListItem>(page, size, (int)Math.Ceiling(total / (double)size), total, items));
	}
}
