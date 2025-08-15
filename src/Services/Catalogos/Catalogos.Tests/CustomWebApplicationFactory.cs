using Catalogos.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Catalogos.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	public Mock<ICatalogosRepository> RepoMock { get; } = new();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			// Reemplaza la implementación real por el mock:
			var descriptor = services.Single(d => d.ServiceType == typeof(ICatalogosRepository));
			services.Remove(descriptor);
			services.AddSingleton<ICatalogosRepository>(_ => RepoMock.Object);
		});
	}
}
