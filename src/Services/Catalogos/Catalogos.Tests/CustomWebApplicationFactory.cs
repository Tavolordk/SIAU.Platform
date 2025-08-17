// Catalogos.Tests/CustomWebApplicationFactory.cs
using Catalogos.Api.Data;
using Catalogos.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Catalogos.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			// Elimina cualquier registro previo
			services.RemoveAll<ICatalogosRepository>();
			// Inyecta el fake
			services.AddSingleton<ICatalogosRepository, FakeCatalogosRepository>();
		});
	}
}
