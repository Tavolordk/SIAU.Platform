using System.Net;
using Newtonsoft.Json.Linq;
using FluentAssertions;

public class PersonasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _client;
	public PersonasControllerTests(CustomWebApplicationFactory f) => _client = f.CreateClient();

	[Fact]
	public async Task Buscar_regresa_200_y_lista()
	{
		var res = await _client.GetAsync("/personas?page=1&pageSize=10");
		res.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = JToken.Parse(await res.Content.ReadAsStringAsync());
		json.Type.Should().Be(JTokenType.Array);
		((JArray)json).Count.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task GetById_regresa_200()
	{
		var res = await _client.GetAsync("/personas/1001");
		res.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Swagger_disponible()
	{
		var res = await _client.GetAsync("/swagger/v1/swagger.json");
		res.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
