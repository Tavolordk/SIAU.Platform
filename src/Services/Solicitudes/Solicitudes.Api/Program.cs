using SharedKernel.Abstractions;
using SharedKernel.Data;
using SharedKernel.Infrastructure.MySql;
using Solicitudes.Application.CambiarEstado;
using Solicitudes.Application.CreateSolicitud;
using Solicitudes.Application.GetSolicitudes;
using Solicitudes.Infrastructure;
using Solicitudes.Infrastructure.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("MySql")!;
var config = builder.Configuration;

builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));
builder.Services.AddSingleton<IStoredProcExecutor>(_ =>
	new MySqlStoredProcExecutor(config.GetConnectionString("Default")!));

builder.Services.AddScoped<ICreateSolicitudHandler, CreateSolicitudHandler>();
builder.Services.AddScoped<IGetSolicitudesPageHandler, GetSolicitudesPageHandler>();
builder.Services.AddScoped<ICambiarEstadoHandler, CambiarEstadoHandler>();

builder.Services.AddControllers().AddJsonOptions(o =>
{
	// si quieres converters para DateOnly, etc.
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
