using Documentos.Api.Data;
using Documentos.Api.Storage;
using Microsoft.OpenApi.Models;
using SharedKernel.Abstractions;
using SharedKernel.Infrastructure.MySql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Documentos API", Version = "v1" });
});

builder.Services.Configure<DocumentosOptions>(
	builder.Configuration.GetSection("Documentos"));

builder.Services.AddSingleton<IStorageProvider, LocalStorageProvider>();
builder.Services.AddScoped<IDocumentosRepository, DocumentosRepository>();

// Registro de conexión MySQL
builder.Services.AddSingleton<IConnectionFactory>(sp =>
	new MySqlConnectionFactory(builder.Configuration.GetConnectionString("MySql")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
