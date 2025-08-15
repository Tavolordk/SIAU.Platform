
namespace Catalogos.Api.Data;

public interface ICatalogosRepository
{
	Task<IEnumerable<object>> Sexos(CancellationToken ct);
	Task<IEnumerable<object>> EstadosCivil(CancellationToken ct);
	Task<IEnumerable<object>> Paises(CancellationToken ct);
	Task<IEnumerable<object>> Nacionalidades(ushort? paisId, CancellationToken ct);
	Task<IEnumerable<object>> Estados(CancellationToken ct);
	Task<IEnumerable<object>> Municipios(uint estadoId, CancellationToken ct);
	Task<IEnumerable<object>> TiposEstructura(CancellationToken ct);
	Task<IEnumerable<object>> Estructura(uint? padreId, byte? tipoId, uint? divisionId, CancellationToken ct);
	Task<IEnumerable<object>> EstadosSolicitud(CancellationToken ct);
	Task<IEnumerable<object>> OpcionesAplican(CancellationToken ct);
	Task<IEnumerable<object>> TiposDocumentos(CancellationToken ct);
	Task<IEnumerable<object>> Sistemas(CancellationToken ct);
	Task<IEnumerable<object>> PerfilesPorSistema(int sistemaId, CancellationToken ct);
}
