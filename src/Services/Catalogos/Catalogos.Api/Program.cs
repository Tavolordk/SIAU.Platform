using Catalogos.Api.Data;
using SharedKernel.Abstractions;
using SharedKernel.Infrastructure.MySql;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.*");

// DI
builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>(); // interfaz -> impl

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
public partial class Program { }
