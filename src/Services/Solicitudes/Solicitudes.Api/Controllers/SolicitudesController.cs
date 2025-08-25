using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Abstractions;
using SharedKernel.Domain.Enums;
using Solicitudes.Api.Dtos;
using Solicitudes.Application.CambiarEstado;
using Solicitudes.Application.CreateSolicitud;
using Solicitudes.Application.GetSolicitudes;
using System.Security.Claims;

namespace Solicitudes.Api.Controllers;

[ApiController]
[Authorize] // ✅ exige JWT en todos los endpoints de este controller
[Route("api/solicitudes")] // opcional: compatibilidad
public sealed class SolicitudesController(
	ICreateSolicitudHandler crear,
	IGetSolicitudesPageHandler listar,
	ICambiarEstadoHandler cambiar,
	IQueryExecutor qx
) : ControllerBase
{
	// ✅ Ya no pedimos UsuarioId en el body
	public sealed record CrearSolicitudDto(uint PersonaId, string EstadoClave, string NumeroOficio, DateOnly FechaSolicitud);
	public sealed record CrearSolicitudResponse(uint SolicitudId, string Folio);

	[HttpPost]
	public async Task<ActionResult<CrearSolicitudResponse>> Crear([FromBody] CrearSolicitudDto dto, CancellationToken ct)
	{
		var usuarioId = GetUserIdOrThrow(User);

		var estado = EstadoSolicitud.FromKey(dto.EstadoClave);
		var r = await crear.Handle(new CreateSolicitudCommand(
			dto.PersonaId, estado, dto.NumeroOficio, dto.FechaSolicitud, usuarioId), ct);

		if (!r.IsSuccess)
			return BadRequest(new { message = r.Error }); // ✅ formato de error

		var (id, folio) = r.Value;
		return CreatedAtAction(nameof(GetById), new { id }, new CrearSolicitudResponse(id, folio));
	}

	[HttpGet]
	public async Task<ActionResult<PageResult<SolicitudListItem>>> GetPage(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10,
		CancellationToken ct = default)
	{
		var r = await listar.Handle(new GetSolicitudesPageQuery(page, pageSize), ct);
		return r.IsSuccess ? Ok(r.Value) : BadRequest(new { message = r.Error }); // ✅ formato de error
	}

	[HttpGet("{id:int:min(0)}")]
	public ActionResult GetById([FromRoute] uint id)
		=> Ok(new { id }); // placeholder (si tienes handler de detalle, úsalo aquí)

	// ✅ Ya no pedimos UsuarioId en el body
	public sealed record CambiarEstadoDto(uint SolicitudId, string EstadoClave, string Comentario);

	[HttpPost("cambiar-estado")]
	public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoDto dto, CancellationToken ct)
	{
		var usuarioId = GetUserIdOrThrow(User);

		var estado = EstadoSolicitud.FromKey(dto.EstadoClave);
		var r = await cambiar.Handle(new CambiarEstadoCommand(dto.SolicitudId, estado, usuarioId, dto.Comentario), ct);

		return r.IsSuccess ? NoContent() : BadRequest(new { message = r.Error }); // ✅ formato de error
	}

	// 🔎 helper para leer el userId del token
	private static uint GetUserIdOrThrow(ClaimsPrincipal user)
	{
		var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirst("id")?.Value;
		if (string.IsNullOrWhiteSpace(idStr))
			throw new UnauthorizedAccessException("Token inválido: no contiene el identificador de usuario.");
		return uint.Parse(idStr);
	}
	[HttpGet("/data/solicitudes")]
	public async Task<ActionResult<PageResultDto<SolicitudLegacyListItemDto>>> GetLegacy(
	   [FromQuery] uint? userId,
	   [FromQuery] int page = 1,
	   [FromQuery] int pageSize = 10,
	   CancellationToken ct = default)
	{
		uint? filterUserId = userId;
		if (filterUserId is null)
		{
			var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;
			if (uint.TryParse(idStr, out var id)) filterUserId = id;
		}

		page = Math.Max(1, page);
		pageSize = Math.Clamp(pageSize, 1, 100);
		var offset = (page - 1) * pageSize;

		var param = new
		{
			userId = (object?)filterUserId ?? System.DBNull.Value, // <- usa System.DBNull
			take = pageSize,
			skip = offset
		};

		const string SqlItems = @"
SELECT
  s.id,
  s.folio,
  s.numero_oficio                    AS Fill1,
  p.nombres                          AS Nombre,
  p.primer_apellido                  AS ApellidoPaterno,
  p.segundo_apellido                 AS ApellidoMaterno,
  COALESCE(u.cuenta, '')             AS CuentaUsuario,
  COALESCE((
      SELECT pc.valor FROM persona_contacto pc
      WHERE pc.persona_id = p.id AND pc.tipo='correo' AND pc.es_principal=1
      ORDER BY pc.validado DESC, pc.id DESC LIMIT 1
  ), '')                             AS CorreoElectronico,
  COALESCE((
      SELECT pc.valor FROM persona_contacto pc
      WHERE pc.persona_id = p.id AND pc.tipo IN ('celular','tel_oficina') AND pc.es_principal=1
      ORDER BY pc.validado DESC, pc.id DESC LIMIT 1
  ), '')                             AS Telefono,
  COALESCE(p.entidad_nacimiento_id,0) AS Entidad,
  YEAR(s.fecha_solicitud)            AS Anio,   -- <- importante para Dapper
  MONTH(s.fecha_solicitud)           AS Mes,
  DAY(s.fecha_solicitud)             AS Dia
FROM solicitudes s
JOIN persona p ON p.id = s.persona_id
LEFT JOIN (
    SELECT t.solicitud_id, t.usuario_id
    FROM solicitud_historial t
    JOIN (
        SELECT solicitud_id, MAX(creado_en) AS max_dt
        FROM solicitud_historial
        WHERE evento='CREACION'
        GROUP BY solicitud_id
    ) m ON m.solicitud_id = t.solicitud_id AND t.creado_en = m.max_dt
    WHERE t.evento='CREACION'
) sh ON sh.solicitud_id = s.id
LEFT JOIN usuarios u ON u.id = sh.usuario_id
WHERE (@userId IS NULL OR sh.usuario_id = @userId)
ORDER BY s.fecha_solicitud DESC
LIMIT @take OFFSET @skip;";

		const string SqlCount = @"
SELECT COUNT(1)
FROM (
  SELECT s.id
  FROM solicitudes s
  LEFT JOIN (
      SELECT t.solicitud_id, t.usuario_id
      FROM solicitud_historial t
      WHERE t.evento='CREACION'
  ) sh ON sh.solicitud_id = s.id
  WHERE (@userId IS NULL OR sh.usuario_id = @userId)
) X;";

		var items = (await qx.QueryAsync<SolicitudLegacyListItemDto>(SqlItems, param, ct)).ToList();
		var total = await qx.QuerySingleAsync<int>(SqlCount, param, ct);
		var totalPages = (int)Math.Ceiling(total / (double)pageSize);

		return Ok(new PageResultDto<SolicitudLegacyListItemDto>
		{
			CurrentPage = page,
			PageSize = pageSize,
			TotalItems = total,
			TotalPages = totalPages,
			Items = items
		});
	}

}
