namespace Personas.Api.Data;

public sealed class PersonaDto
{
	public PersonaDto() { } // ctor explícito

	public uint Id { get; set; }
	public string Nombres { get; set; } = "";
	public string PrimerApellido { get; set; } = "";
	public string? SegundoApellido { get; set; }
	public sbyte? SexoId { get; set; }
	public DateOnly FechaNacimiento { get; set; }
	public ushort? NacionalidadId { get; set; }
	public ushort? PaisNacimientoId { get; set; }
	public uint? EntidadNacimientoId { get; set; }
	public uint? MunicipioNacimientoId { get; set; }
	public sbyte? EstadoCivilId { get; set; }
	public string Rfc { get; set; } = "";
	public string Curp { get; set; } = "";
	public bool CurpValidada { get; set; }
}
