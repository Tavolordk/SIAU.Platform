using MySql.Data.MySqlClient;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using System.Data;
using Solicitudes.Application.GetSolicitudes;

namespace Solicitudes.Infrastructure;

public sealed class GetSolicitudesPageHandler(IConnectionFactory factory) : IGetSolicitudesPageHandler
{
	public async Task<Result<PageResult<SolicitudListItem>>> Handle(GetSolicitudesPageQuery q, CancellationToken ct)
	{
		var page = Math.Max(1, q.Page);
		var size = Math.Clamp(q.PageSize, 1, 100);
		var offset = (page - 1) * size;

		using var conn = (MySqlConnection)factory.Create();
		await conn.OpenAsync(ct);

		int total;
		using (var countCmd = new MySqlCommand("SELECT COUNT(1) FROM solicitudes", conn))
		{ total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct)); }

		var items = new List<SolicitudListItem>();
		using (var cmd = new MySqlCommand(@"
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
            LIMIT @limit OFFSET @offset
        ", conn))
		{
			cmd.Parameters.AddWithValue("@limit", size);
			cmd.Parameters.AddWithValue("@offset", offset);
			using var r = await cmd.ExecuteReaderAsync(ct);
			while (await r.ReadAsync(ct))
			{
				items.Add(new SolicitudListItem(
                    Convert.ToUInt32(r.GetInt32("id")),
                    r.GetString("folio"),
					r.GetString("persona_nombre"),
					r.GetString("estado_clave"),
					DateOnly.FromDateTime(r.GetDateTime("fecha_solicitud")),
					r.GetString("numero_oficio"),
					r.GetDateTime("created_at")
				));
			}
		}

		var totalPages = (int)Math.Ceiling(total / (double)size);
		return Result<PageResult<SolicitudListItem>>.Success(
			new PageResult<SolicitudListItem>(page, size, totalPages, total, items));
	}
}
