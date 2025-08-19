using Microsoft.AspNetCore.Mvc;
using Personas.Api.Data;

namespace Personas.Api.Controllers;

[ApiController]
[Route("personas")]
public sealed class PersonasController(IPersonasRepository repo) : ControllerBase
{
	[HttpGet("{id:int:min(1)}")]
	public Task<PersonaDto?> GetById([FromRoute] int id, CancellationToken ct)
		=> repo.GetById((uint)id, ct);

	[HttpGet]
	public Task<IReadOnlyList<PersonaDto>> Search(
		[FromQuery] string? texto, [FromQuery] string? curp, [FromQuery] string? rfc,
		[FromQuery] int page = 1, [FromQuery] int pageSize = 20,
		CancellationToken ct = default)
		=> repo.Search(texto, curp, rfc, page, Math.Clamp(pageSize, 1, 100), ct);

	[HttpPost]
	public async Task<ActionResult<object>> Create([FromBody] PersonaCreateDto dto, CancellationToken ct)
	{
		var id = await repo.Create(dto, ct);
		return CreatedAtAction(nameof(GetById), new { id }, new { id });
	}

	[HttpGet("{id:int:min(1)}/contactos")]
	public Task<IReadOnlyList<object>> Contactos([FromRoute] int id, CancellationToken ct)
		=> repo.GetContactos((uint)id, ct);

	[HttpPost("{id:int:min(1)}/contactos")]
	public async Task<IActionResult> AddContacto([FromRoute] int id, [FromBody] ContactoCreateDto dto, CancellationToken ct)
	{
		await repo.AddContacto((uint)id, dto, ct);
		return NoContent();
	}

	[HttpGet("{id:int:min(1)}/asignaciones")]
	public Task<IReadOnlyList<object>> Asignaciones([FromRoute] int id, [FromQuery] bool? soloActivas, CancellationToken ct)
		=> repo.GetAsignaciones((uint)id, soloActivas, ct);

	[HttpPost("{id:int:min(1)}/asignaciones")]
	public async Task<IActionResult> AddAsignacion(
		[FromRoute] int id,
		[FromBody] AsignacionCreateDto dto,
		CancellationToken ct)
	{
		await repo.AddAsignacion((uint)id, dto, ct);
		return NoContent();
	}


}