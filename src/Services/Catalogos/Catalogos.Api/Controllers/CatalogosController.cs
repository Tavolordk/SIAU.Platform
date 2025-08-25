using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Dapper;
using System.Data;
using MySqlConnector; // o el provider que uses

namespace Catalogos.Api.Controllers;

[ApiController]
[Authorize]                  // igual que el resto, si tu API pide JWT
[Route("catalogos")]
public sealed class CatalogosController : ControllerBase
{
	private readonly IDbConnection _db;

	public CatalogosController(IDbConnection db)   // <- inyectada desde Program.cs
	{
		_db = db;
	}
	// ===== DTOs con nombres en MAYÚSCULAS (no rompemos camelCase global) =====
	public sealed class TipoUsuarioDto { public int ID { get; set; } public string TP_USUARIO { get; set; } = ""; }
	public sealed class CatEntiDto { public int ID { get; set; } public string NOMBRE { get; set; } = ""; public string TIPO { get; set; } = ""; public int? FK_PADRE { get; set; } }
	public sealed class CatEstructuraDto { public int ID { get; set; } public string NOMBRE { get; set; } = ""; public string TIPO { get; set; } = ""; public int? FK_PADRE { get; set; } }
	public sealed class CatPerfilDto { public int ID { get; set; } public string CLAVE { get; set; } = ""; public string FUNCION { get; set; } = ""; }

	public sealed class CatalogosResponseDto
	{
		[JsonPropertyName("TipoUsuario")] public List<TipoUsuarioDto> TipoUsuario { get; set; } = [];
		[JsonPropertyName("Entidades")] public List<CatEntiDto> Entidades { get; set; } = [];
		[JsonPropertyName("Estructura")] public List<CatEstructuraDto> Estructura { get; set; } = [];
		[JsonPropertyName("Perfiles")] public List<CatPerfilDto> Perfiles { get; set; } = [];
	}

	[HttpGet("tpusuario")]
	public async Task<ActionResult<CatalogosResponseDto>> GetTpUsuario(CancellationToken ct)
	{
		const string sqlTipos = @"
    SELECT id AS ID, descripcion AS TP_USUARIO
    FROM cat_tp_usuarios
    ORDER BY id;";

		const string sqlEntidades = @"
    SELECT e.id AS ID, e.nombre AS NOMBRE, 'ESTADO' AS TIPO, NULL AS FK_PADRE
    FROM cat_division_geopolitica e
    WHERE e.nivel = 'estado'
    UNION ALL
    SELECT m.id AS ID, m.nombre AS NOMBRE, 'MUNICIPIO' AS TIPO, m.fk_padre AS FK_PADRE
    FROM cat_division_geopolitica m
    WHERE m.nivel = 'municipio'
    ORDER BY TIPO, NOMBRE;";

		const string sqlEstructura = @"
    SELECT id AS ID, nombre AS NOMBRE, tipo_ID AS TIPO, fk_padre AS FK_PADRE
    FROM cat_estructura_organizacional
    ORDER BY TIPO_ID, NOMBRE;";

		const string sqlPerfiles = @"
    SELECT id AS ID, clave AS CLAVE, descripcion AS FUNCION
    FROM cat_perfiles
    WHERE activo = 1
    ORDER BY id;";



		var r = new CatalogosResponseDto
		{
			TipoUsuario = (await _db.QueryAsync<TipoUsuarioDto>(new CommandDefinition(sqlTipos, cancellationToken: ct))).ToList(),
			Entidades = (await _db.QueryAsync<CatEntiDto>(new CommandDefinition(sqlEntidades, cancellationToken: ct))).ToList(),
			Estructura = (await _db.QueryAsync<CatEstructuraDto>(new CommandDefinition(sqlEstructura, cancellationToken: ct))).ToList(),
			Perfiles = (await _db.QueryAsync<CatPerfilDto>(new CommandDefinition(sqlPerfiles, cancellationToken: ct))).ToList()
		};

		return Ok(r);
	}
}
