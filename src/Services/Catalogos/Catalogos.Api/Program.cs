// Catalogos.Api/Program.cs
using Catalogos.Api.Data;
using SharedKernel.Abstractions;
using SharedKernel.Infrastructure.MySql;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql");

// Infra
builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));

// Repo
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();

// Web + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();

// Necesario para WebApplicationFactory
public partial class Program { }
