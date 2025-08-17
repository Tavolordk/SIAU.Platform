using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Catalogos.Tests;

public class CatalogosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _client;
	public CatalogosControllerTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

	public static IEnumerable<object[]> Rutas() => new[]
	{
		new object[] { "/catalogos/sexos" },
		new object[] { "/catalogos/estados-civil" },
		new object[] { "/catalogos/paises" },
		new object[] { "/catalogos/nacionalidades" },
		new object[] { "/catalogos/estados" },
		new object[] { "/catalogos/estados/9/municipios" },
		new object[] { "/catalogos/tipos-estructura" },
		new object[] { "/catalogos/estructura" },
		new object[] { "/catalogos/estados-solicitud" },
		new object[] { "/catalogos/opciones-aplican" },
		new object[] { "/catalogos/tipos-documentos" },
		new object[] { "/catalogos/sistemas" },
		new object[] { "/catalogos/sistemas/1/perfiles" },
	};

	[Theory]
	[MemberData(nameof(Rutas))]
	public async Task Endpoints_de_catalogos_regresan_200_y_lista(string url)
	{
		var res = await _client.GetAsync(url);
		res.StatusCode.Should().Be(HttpStatusCode.OK);

		var txt = await res.Content.ReadAsStringAsync();
		var json = JToken.Parse(txt);

		json.Type.Should().Be(JTokenType.Array);
		((JArray)json).Count.Should().BeGreaterThan(0); // mejor > 0 que == 1
	}

	[Fact]
	public async Task Swagger_esta_habilitado()
	{
		var res = await _client.GetAsync("/swagger/v1/swagger.json");
		res.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
