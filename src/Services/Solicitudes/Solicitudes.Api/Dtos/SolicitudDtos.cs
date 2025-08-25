namespace Solicitudes.Api.Dtos
{
	public class CrearSolicitudRequest
	{
		public int AreaId { get; set; }
		public string Asunto { get; set; } = string.Empty;
		public string Descripcion { get; set; } = string.Empty;
		public string Prioridad { get; set; } = "MEDIA"; // ALTA|MEDIA|BAJA
		public List<AdjuntoDto>? Adjuntos { get; set; }
	}

	public class AdjuntoDto
	{
		public string Nombre { get; set; } = string.Empty;
		public string Mime { get; set; } = "application/octet-stream";
		public string Base64 { get; set; } = string.Empty;
	}

	public class CrearSolicitudResponse
	{
		public int SolicitudId { get; set; }
		public string Folio { get; set; } = string.Empty;
	}

	public class Paginado<T>
	{
		public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
		public int Total { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	}

	public class SolicitudResumenDto
	{
		public int Id { get; set; }
		public string Folio { get; set; } = string.Empty;
		public string Asunto { get; set; } = string.Empty;
		public string Estado { get; set; } = "ENVIADA";
		public string Prioridad { get; set; } = "MEDIA";
		public string CreadoPor { get; set; } = string.Empty;
		public DateTime FechaCreacion { get; set; }
	}

	public class SolicitudDetalleDto
	{
		public int Id { get; set; }
		public string Folio { get; set; } = string.Empty;
		public string Asunto { get; set; } = string.Empty;
		public string Descripcion { get; set; } = string.Empty;
		public string Estado { get; set; } = "ENVIADA";
		public string Prioridad { get; set; } = "MEDIA";
		public int AreaId { get; set; }
		public int CreadoPorUserId { get; set; }
		public string CreadoPor { get; set; } = string.Empty;
		public DateTime FechaCreacion { get; set; }
		public List<AdjuntoOutDto> Adjuntos { get; set; } = new();
		public List<MovimientoDto> Historial { get; set; } = new();
	}

	public class AdjuntoOutDto
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Mime { get; set; } = "application/pdf";
		public string Url { get; set; } = string.Empty;
	}

	public class MovimientoDto
	{
		public DateTime Fecha { get; set; }
		public string Accion { get; set; } = string.Empty;
		public string Usuario { get; set; } = string.Empty;
	}

	public class CambiarEstadoRequest
	{
		public string Estado { get; set; } = string.Empty; // EN_PROCESO | RECHAZADA | CERRADA
		public string? Motivo { get; set; }
	}
}
