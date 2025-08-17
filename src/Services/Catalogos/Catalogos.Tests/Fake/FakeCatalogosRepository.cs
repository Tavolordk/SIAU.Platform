// Catalogos.Tests/Fakes/FakeCatalogosRepository.cs
using Catalogos.Api.Data;

namespace Catalogos.Tests.Fakes;

public sealed class FakeCatalogosRepository : ICatalogosRepository
{
	public Task<IEnumerable<object>> Sexos(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, nombre = "MASCULINO" } });

	public Task<IEnumerable<object>> EstadosCivil(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, nombre = "SOLTERO" } });

	public Task<IEnumerable<object>> Paises(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 484, nombre = "México" } });

	public Task<IEnumerable<object>> Nacionalidades(ushort? _, CancellationToken __) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, nombre = "Mexicana", paisId = 484 } });

	public Task<IEnumerable<object>> Estados(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 9u, nombre = "Ciudad de México" } });

	public Task<IEnumerable<object>> Municipios(uint _, CancellationToken __) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 9001u, nombre = "Álvaro Obregón" } });

	public Task<IEnumerable<object>> TiposEstructura(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = (byte)1, nombre = "Institución" } });

	public Task<IEnumerable<object>> Estructura(uint? _, byte? __, uint? ___, CancellationToken ____) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 100u, nombre = "Secretaría X", tipoId = (byte)1, padreId = (uint?)null, divisionId = (uint?)null } });

	public Task<IEnumerable<object>> EstadosSolicitud(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, clave = "CAPTURA", descripcion = "Captura" } });

	public Task<IEnumerable<object>> OpcionesAplican(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, clave = "OPC_A", nombre = "Opción A" } });

	public Task<IEnumerable<object>> TiposDocumentos(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, nombre = "INE" } });

	public Task<IEnumerable<object>> Sistemas(CancellationToken _) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, nombre = "SIGE" } });

	public Task<IEnumerable<object>> PerfilesPorSistema(int _, CancellationToken __) =>
		Task.FromResult<IEnumerable<object>>(new[] { new { id = 1, clave = "ADMIN", descripcion = "Administrador" } });
}
