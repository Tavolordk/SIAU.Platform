using SharedKernel.Abstractions;                 // IConnectionFactory
using SharedKernel.Data;                        // IQueryExecutor, IStoredProcExecutor
using SharedKernel.Infrastructure.MySql;        // MySqlConnectionFactory

using Solicitudes.Application.CambiarEstado;    // ICambiarEstadoHandler
using Solicitudes.Application.CreateSolicitud;  // ICreateSolicitudHandler, CreateSolicitudHandler
using Solicitudes.Application.GetSolicitudes;   // IGetSolicitudesPageHandler
using Solicitudes.Infrastructure;               // CambiarEstadoHandler, GetSolicitudesPageHandler
using Solicitudes.Infrastructure.Data;          // MySqlQueryExecutor, MySqlStoredProcExecutor

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string (appsettings.json -> "ConnectionStrings": { "MySql": "..." })
var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.*");

// 2) Infraestructura MySQL
builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));
builder.Services.AddScoped<IQueryExecutor, MySqlQueryExecutor>();
builder.Services.AddSingleton<IStoredProcExecutor>(_ => new MySqlStoredProcExecutor(cs));

// 3) Application (handlers)
builder.Services.AddScoped<ICreateSolicitudHandler, CreateSolicitudHandler>();
builder.Services.AddScoped<IGetSolicitudesPageHandler, GetSolicitudesPageHandler>();
builder.Services.AddScoped<ICambiarEstadoHandler, CambiarEstadoHandler>();

// 4) Web API
builder.Services.AddControllers();             // En .NET 8/9, DateOnly/TimeOnly ya tienen soporte
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger siempre habilitado (ajústalo si quieres condicionar por entorno)
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
