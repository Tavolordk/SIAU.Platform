using System.Net;
using FluentAssertions;

namespace Catalogos.Api.Tests;

public class PingControllerTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _client;
	public PingControllerTests(CustomWebApplicationFactory f) => _client = f.CreateClient();

	[Fact]
	public async Task Ping_regresa_200()
	{
		var res = await _client.GetAsync("/ping");
		res.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
