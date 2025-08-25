using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using SharedKernel.Abstractions;                 // IConnectionFactory
using SharedKernel.Data;                        // IQueryExecutor, IStoredProcExecutor
using SharedKernel.Infrastructure.MySql;        // MySqlConnectionFactory

using Solicitudes.Application.CambiarEstado;    // ICambiarEstadoHandler
using Solicitudes.Application.CreateSolicitud;  // ICreateSolicitudHandler, CreateSolicitudHandler
using Solicitudes.Application.GetSolicitudes;   // IGetSolicitudesPageHandler
using Solicitudes.Infrastructure;               // CambiarEstadoHandler, GetSolicitudesPageHandler
using Solicitudes.Infrastructure.Data;          // MySqlQueryExecutor, MySqlStoredProcExecutor

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string
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

// 4) Web API (JSON camelCase)
builder.Services.AddControllers()
	.AddJsonOptions(o => { o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5) CORS para Angular dev
builder.Services.AddCors(opt =>
{
	opt.AddPolicy("AllowAngularDev", p =>
		p.WithOrigins("https://localhost:4443", "http://localhost:4200")
		 .AllowAnyHeader()
		 .AllowAnyMethod());
});

// 6) JWT Bearer (usa MISMA Key/Issuer/Audience que el micro de Usuarios)
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(o =>
	{
		o.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = issuer,
			ValidAudience = audience,
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ClockSkew = TimeSpan.Zero
		};
	});

// (Opcional) Forzar auth por defecto si olvidas [Authorize] en algún endpoint
// builder.Services.AddAuthorization(o =>
//     o.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
//         .RequireAuthenticatedUser().Build());

var app = builder.Build();

// Swagger (como lo tenías)
app.UseSwagger();
app.UseSwaggerUI();

// Pipeline correcto
app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthentication();   // <-- IMPORTANTE: antes de Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
