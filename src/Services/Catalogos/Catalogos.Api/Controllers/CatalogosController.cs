using Microsoft.AspNetCore.Mvc;
using Catalogos.Api.Data;

namespace Catalogos.Api.Controllers;

[ApiController]
[Route("catalogos")]
public sealed class CatalogosController(ICatalogosRepository repo) : ControllerBase
{
	[HttpGet("sexos")] public Task<IEnumerable<object>> Sexos(CancellationToken ct) => repo.Sexos(ct);
	[HttpGet("estados-civil")] public Task<IEnumerable<object>> EstadosCivil(CancellationToken ct) => repo.EstadosCivil(ct);
	[HttpGet("paises")] public Task<IEnumerable<object>> Paises(CancellationToken ct) => repo.Paises(ct);

	[HttpGet("nacionalidades")]
	public Task<IEnumerable<object>> Nacionalidades([FromQuery] ushort? paisId, CancellationToken ct)
		=> repo.Nacionalidades(paisId, ct);

	[HttpGet("estados")] public Task<IEnumerable<object>> Estados(CancellationToken ct) => repo.Estados(ct);

	[HttpGet("estados/{estadoId}/municipios")]
	public Task<IEnumerable<object>> Municipios([FromRoute] uint estadoId, CancellationToken ct)
		=> repo.Municipios(estadoId, ct);

	[HttpGet("tipos-estructura")] public Task<IEnumerable<object>> TiposEstructura(CancellationToken ct) => repo.TiposEstructura(ct);

	[HttpGet("estructura")]
	public Task<IEnumerable<object>> Estructura([FromQuery] uint? padreId, [FromQuery] byte? tipoId, [FromQuery] uint? divisionId, CancellationToken ct)
		=> repo.Estructura(padreId, tipoId, divisionId, ct);

	[HttpGet("estados-solicitud")] public Task<IEnumerable<object>> EstadosSolicitud(CancellationToken ct) => repo.EstadosSolicitud(ct);
	[HttpGet("opciones-aplican")] public Task<IEnumerable<object>> OpcionesAplican(CancellationToken ct) => repo.OpcionesAplican(ct);
	[HttpGet("tipos-documentos")] public Task<IEnumerable<object>> TiposDocumentos(CancellationToken ct) => repo.TiposDocumentos(ct);

	[HttpGet("sistemas")] public Task<IEnumerable<object>> Sistemas(CancellationToken ct) => repo.Sistemas(ct);

	[HttpGet("sistemas/{sistemaId}/perfiles")]
	public Task<IEnumerable<object>> PerfilesPorSistema([FromRoute] int sistemaId, CancellationToken ct)
		=> repo.PerfilesPorSistema(sistemaId, ct);
}
