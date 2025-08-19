namespace Personas.Api.Data;


public record PersonaCreateDto(
	string Nombres, string PrimerApellido, string? SegundoApellido,
	byte? SexoId, DateOnly FechaNacimiento,
	ushort? NacionalidadId, ushort? PaisNacimientoId,
	uint? EntidadNacimientoId, uint? MunicipioNacimientoId,
	byte? EstadoCivilId, string Rfc, string Curp);

public record ContactoCreateDto(string Tipo, string Valor, string? Extension, bool EsPrincipal);

public interface IPersonasRepository
{
	Task<PersonaDto?> GetById(uint id, CancellationToken ct);
	Task<uint> Create(PersonaCreateDto dto, CancellationToken ct);
	Task<IReadOnlyList<PersonaDto>> Search(string? texto, string? curp, string? rfc, int page, int pageSize, CancellationToken ct);

	Task AddContacto(uint personaId, ContactoCreateDto dto, CancellationToken ct);
	Task<IReadOnlyList<object>> GetContactos(uint personaId, CancellationToken ct);

	Task AddAsignacion(uint personaId, AsignacionCreateDto dto, CancellationToken ct);
	Task<IReadOnlyList<object>> GetAsignaciones(uint personaId, bool? soloActivas, CancellationToken ct);
}