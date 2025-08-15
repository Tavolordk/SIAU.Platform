using Catalogos.Api.Data;
using SharedKernel.Abstractions;
using SharedKernel.Infrastructure.MySql;

var builder = WebApplication.CreateBuilder(args);

// 1) DB
var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.json");

// IConnectionFactory que devuelve MySqlConnector.MySqlConnection
builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));

// 2) Repositorios (scoped por request)
builder.Services.AddScoped<CatalogosRepository>();
// Si tienes interfaz, usa esto:
// builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();

// 3) Web + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catálogos API v1");
	// c.RoutePrefix = string.Empty; // opcional: UI en "/"
});

app.MapControllers();
app.Run();
