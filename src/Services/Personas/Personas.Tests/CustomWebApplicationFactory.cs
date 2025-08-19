using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Personas.Api.Data;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			services.AddScoped<IPersonasRepository>(_ => new FakeRepo());
		});
	}

	private sealed class FakeRepo : IPersonasRepository
	{
		private readonly List<PersonaDto> _personas = new()
		{
			new PersonaDto(1001, "JUAN", "PEREZ", "LOPEZ", 1, new DateOnly(1990,1,1), 484, 484, null, null, 1, "PELJ900101XXX", "PELG900101HDFRPN09", false)
		};

		public Task<uint> Create(PersonaCreateDto dto, CancellationToken ct)
		{
			var id = (uint)(_personas.Max(p => (int)p.Id) + 1);
			_personas.Add(new PersonaDto(id, dto.Nombres, dto.PrimerApellido, dto.SegundoApellido, dto.SexoId,
				dto.FechaNacimiento, dto.NacionalidadId, dto.PaisNacimientoId, dto.EntidadNacimientoId,
				dto.MunicipioNacimientoId, dto.EstadoCivilId, dto.Rfc, dto.Curp, false));
			return Task.FromResult(id);
		}

		public Task<PersonaDto?> GetById(uint id, CancellationToken ct) =>
			Task.FromResult(_personas.FirstOrDefault(p => p.Id == id));

		public Task<IReadOnlyList<PersonaDto>> Search(string? texto, string? curp, string? rfc, int page, int pageSize, CancellationToken ct)
			=> Task.FromResult<IReadOnlyList<PersonaDto>>(_personas);

		public Task AddContacto(uint personaId, ContactoCreateDto dto, CancellationToken ct) => Task.CompletedTask;
		public Task<IReadOnlyList<object>> GetContactos(uint personaId, CancellationToken ct)
			=> Task.FromResult<IReadOnlyList<object>>(new object[] { new { id = 1, tipo = "correo", valor = "demo@x.com" } });

		public Task AddAsignacion(uint personaId, AsignacionCreateDto dto, CancellationToken ct) => Task.CompletedTask;
		public Task<IReadOnlyList<object>> GetAsignaciones(uint personaId, bool? soloActivas, CancellationToken ct)
			=> Task.FromResult<IReadOnlyList<object>>(new object[] { new { id = 1, estructuraId = 10, tipoAsignacion = "adscripcion" } });
	}
}