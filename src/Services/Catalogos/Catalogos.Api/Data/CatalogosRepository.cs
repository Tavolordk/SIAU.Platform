using System.Data;
using System.Data.Common;
using Dapper;
using SharedKernel.Abstractions;

namespace Catalogos.Api.Data;

public sealed class CatalogosRepository(IConnectionFactory factory)
{
	private async Task<T> WithConn<T>(Func<IDbConnection, Task<T>> work, CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct);
		else conn.Open();
		return await work(conn);
	}

	public Task<IEnumerable<object>> Sexos(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, descripcion AS nombre FROM cat_sexo WHERE activo=1 ORDER BY id"),
			ct);

	public Task<IEnumerable<object>> EstadosCivil(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, descripcion AS nombre FROM cat_estado_civil WHERE activo=1 ORDER BY id"),
			ct);

	public Task<IEnumerable<object>> Paises(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, nombre FROM cat_pais WHERE activo=1 ORDER BY nombre"),
			ct);

	public Task<IEnumerable<object>> Nacionalidades(ushort? paisId, CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			@"SELECT id, gentilicio AS nombre, pais_id AS paisId
              FROM cat_nacionalidad
              WHERE activo=1 AND (@paisId IS NULL OR pais_id=@paisId)
              ORDER BY nombre",
			new { paisId }), ct);

	public Task<IEnumerable<object>> Estados(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			@"SELECT id, nombre FROM cat_division_geopolitica 
              WHERE activo=1 AND nivel='estado' ORDER BY nombre"),
			ct);

	public Task<IEnumerable<object>> Municipios(uint estadoId, CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			@"SELECT id, nombre FROM cat_division_geopolitica
              WHERE activo=1 AND nivel='municipio' AND fk_padre=@estadoId
              ORDER BY nombre",
			new { estadoId }), ct);

	public Task<IEnumerable<object>> TiposEstructura(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, nombre FROM cat_tipo_estructura ORDER BY id"), ct);

	public Task<IEnumerable<object>> Estructura(uint? padreId, byte? tipoId, uint? divisionId, CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			@"SELECT id, nombre, tipo_id AS tipoId, fk_padre AS padreId, fk_division_geopolitica AS divisionId
              FROM cat_estructura_organizacional
              WHERE (@padreId IS NULL AND fk_padre IS NULL OR fk_padre=@padreId)
                AND (@tipoId IS NULL OR tipo_id=@tipoId)
                AND (@divisionId IS NULL OR fk_division_geopolitica=@divisionId)
              ORDER BY nombre",
			new { padreId, tipoId, divisionId }), ct);

	public Task<IEnumerable<object>> EstadosSolicitud(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, clave, descripcion FROM cat_estado_solicitudes WHERE activo=1 ORDER BY id"), ct);

	public Task<IEnumerable<object>> OpcionesAplican(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, clave, nombre FROM cat_opciones_aplican WHERE activo=1 ORDER BY id"), ct);

	public Task<IEnumerable<object>> TiposDocumentos(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, nombre FROM cat_tipos_documentos WHERE activo=1 ORDER BY id"), ct);

	public Task<IEnumerable<object>> Sistemas(CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			"SELECT id, sistema AS nombre FROM cat_sistemas WHERE activo=1 ORDER BY nombre"), ct);

	public Task<IEnumerable<object>> PerfilesPorSistema(int sistemaId, CancellationToken ct) =>
		WithConn(conn => conn.QueryAsync(
			@"SELECT p.id, p.clave, p.descripcion
              FROM cat_perfiles p
              JOIN sistemas_perfiles sp ON sp.id_perfil = p.id
              WHERE sp.id_sistema=@sistemaId AND p.activo=1
              ORDER BY p.descripcion",
			new { sistemaId }), ct);
}
