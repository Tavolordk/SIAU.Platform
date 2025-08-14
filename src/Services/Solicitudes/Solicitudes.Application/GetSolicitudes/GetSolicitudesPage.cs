using SharedKernel.Results;

namespace Solicitudes.Application.GetSolicitudes;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public sealed record GetSolicitudesPageQuery(int Page, int PageSize);
[ExcludeFromCodeCoverage]

public sealed record SolicitudListItem(
	uint Id, string Folio, string PersonaNombre, string EstadoClave,
	DateOnly FechaSolicitud, string NumeroOficio, DateTime CreatedAt
);
[ExcludeFromCodeCoverage]

public sealed record PageResult<T>(int CurrentPage, int PageSize, int TotalPages, int TotalItems, IReadOnlyList<T> Items);

public interface IGetSolicitudesPageHandler
{
	Task<Result<PageResult<SolicitudListItem>>> Handle(GetSolicitudesPageQuery q, CancellationToken ct);
}
