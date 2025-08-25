using System.Text.Json.Serialization;

namespace Solicitudes.Api.Dtos
{
	public sealed class SolicitudLegacyListItemDto
	{
		public uint Id { get; set; }
		public string Fill1 { get; set; } = string.Empty;          // ← numero_oficio
		public string Folio { get; set; } = string.Empty;

		public string Nombre { get; set; } = string.Empty;          // persona.nombres
		public string ApellidoPaterno { get; set; } = string.Empty; // persona.primer_apellido
		public string? ApellidoMaterno { get; set; }                // persona.segundo_apellido

		public string CuentaUsuario { get; set; } = string.Empty;   // creador (historial)
		public string CorreoElectronico { get; set; } = string.Empty; // principal (si existe)
		public string Telefono { get; set; } = string.Empty;          // principal (si existe)

		public int Entidad { get; set; }                            // persona.entidad_nacimiento_id}

		[JsonPropertyName("año")] public int Anio { get; set; }
		public int Mes { get; set; }
		public int Dia { get; set; }

		// Si tu UI no los usa en la lista, puedes dejarlos en default:
		public bool CheckBox1 { get; set; } = false;
		public bool CheckBox2 { get; set; } = false;
		public bool CheckBox3 { get; set; } = false;
		public bool CheckBox4 { get; set; } = false;
		public bool CheckBox5 { get; set; } = false;
	}

	public sealed class PageResultDto<T>
	{
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int TotalItems { get; set; }
		public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
	}
}
