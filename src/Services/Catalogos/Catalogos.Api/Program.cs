using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Data;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string (MISMA clave que usas en appsettings: ConnectionStrings:MySql)
var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.*");

// 2) Inyección de conexión MySQL para Dapper
builder.Services.AddScoped<IDbConnection>(_ => new MySqlConnection(cs));

// 3) Web API (JSON camelCase)
builder.Services.AddControllers()
	.AddJsonOptions(o => { o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4) CORS (igual que Solicitudes)
builder.Services.AddCors(opt =>
{
	opt.AddPolicy("AllowAngularDev", p =>
		p.WithOrigins("https://localhost:4443", "http://localhost:4200")
		 .AllowAnyHeader()
		 .AllowAnyMethod());
});

// 5) JWT (idéntico a Solicitudes)
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

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
