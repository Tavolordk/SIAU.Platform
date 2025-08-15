using System.Net;
using System.Net.Http.Json;
using Catalogos.Api.Data;
using FluentAssertions;
using Moq;

namespace Catalogos.Api.Tests;

public class CatalogosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly CustomWebApplicationFactory _factory;
	private readonly HttpClient _client;
	private readonly Mock<ICatalogosRepository> _repo;

	public CatalogosControllerTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});
		_repo = factory.RepoMock;

		// Respuestas por defecto para no fallar si olvido configurar alguna:
		var ok = new[] { new Dictionary<string, object> { ["id"] = 1, ["nombre"] = "OK" } }.Cast<object>();
		_repo.Setup(r => r.Sexos(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.EstadosCivil(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Paises(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Nacionalidades(It.IsAny<ushort?>(), default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Estados(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Municipios(It.IsAny<uint>(), default)).ReturnsAsync(ok);
		_repo.Setup(r => r.TiposEstructura(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Estructura(It.IsAny<uint?>(), It.IsAny<byte?>(), It.IsAny<uint?>(), default)).ReturnsAsync(ok);
		_repo.Setup(r => r.EstadosSolicitud(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.OpcionesAplican(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.TiposDocumentos(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.Sistemas(default)).ReturnsAsync(ok);
		_repo.Setup(r => r.PerfilesPorSistema(It.IsAny<int>(), default)).ReturnsAsync(ok);
	}

	[Theory]
	[InlineData("/catalogos/sexos")]
	[InlineData("/catalogos/estados-civil")]
	[InlineData("/catalogos/paises")]
	[InlineData("/catalogos/nacionalidades?paisId=484")]
	[InlineData("/catalogos/estados")]
	[InlineData("/catalogos/estados/10/municipios")]
	[InlineData("/catalogos/tipos-estructura")]
	[InlineData("/catalogos/estructura?padreId=1&tipoId=2&divisionId=3")]
	[InlineData("/catalogos/estados-solicitud")]
	[InlineData("/catalogos/opciones-aplican")]
	[InlineData("/catalogos/tipos-documentos")]
	[InlineData("/catalogos/sistemas")]
	[InlineData("/catalogos/sistemas/5/perfiles")]
	public async Task Endpoints_de_catalogos_regresan_200_y_lista(string url)
	{
		var res = await _client.GetAsync(url);
		res.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await res.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
		json.Should().NotBeNull();
		json!.Count.Should().Be(1);
		json[0].Should().ContainKey("id");
	}

	[Fact]
	public async Task Swagger_esta_habilitado()
	{
		var res = await _client.GetAsync("/swagger/v1/swagger.json");
		res.StatusCode.Should().Be(HttpStatusCode.OK);
		var swagger = await res.Content.ReadAsStringAsync();
		swagger.Should().Contain("\"openapi\"");
	}
}
