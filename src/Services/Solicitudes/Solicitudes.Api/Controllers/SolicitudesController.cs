using Microsoft.AspNetCore.Mvc;
using SharedKernel.Domain.Enums;
using Solicitudes.Application.CreateSolicitud;
using Solicitudes.Application.GetSolicitudes;
using Solicitudes.Application.CambiarEstado;

namespace Solicitudes.Api.Controllers;

[ApiController]
[Route("solicitudes")]
public sealed class SolicitudesController(
	ICreateSolicitudHandler crear,
	IGetSolicitudesPageHandler listar,
	ICambiarEstadoHandler cambiar
) : ControllerBase
{
	public sealed record CrearSolicitudDto(uint PersonaId, string EstadoClave, string NumeroOficio, DateOnly FechaSolicitud, uint UsuarioId);
	public sealed record CrearSolicitudResponse(uint SolicitudId, string Folio);

	[HttpPost]
	public async Task<ActionResult<CrearSolicitudResponse>> Crear([FromBody] CrearSolicitudDto dto, CancellationToken ct)
	{
		var estado = EstadoSolicitud.FromKey(dto.EstadoClave);
		var r = await crear.Handle(new CreateSolicitudCommand(dto.PersonaId, estado, dto.NumeroOficio, dto.FechaSolicitud, dto.UsuarioId), ct);
		if (!r.IsSuccess) return BadRequest(r.Error);
		var (id, folio) = r.Value;
		return CreatedAtAction(nameof(GetById), new { id }, new CrearSolicitudResponse(id, folio));
	}

	[HttpGet]
	public async Task<ActionResult<PageResult<SolicitudListItem>>> GetPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
	{
		var r = await listar.Handle(new GetSolicitudesPageQuery(page, pageSize), ct);
		return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
	}

	[HttpGet("{id:uint}")]
	public ActionResult GetById([FromRoute] uint id) => Ok(new { id }); // placeholder

	public sealed record CambiarEstadoDto(uint SolicitudId, string EstadoClave, uint UsuarioId, string Comentario);

	[HttpPost("cambiar-estado")]
	public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoDto dto, CancellationToken ct)
	{
		var estado = EstadoSolicitud.FromKey(dto.EstadoClave);
		var r = await cambiar.Handle(new CambiarEstadoCommand(dto.SolicitudId, estado, dto.UsuarioId, dto.Comentario), ct);
		return r.IsSuccess ? NoContent() : BadRequest(r.Error);
	}
}
