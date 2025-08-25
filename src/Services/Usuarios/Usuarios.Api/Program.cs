using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Usuarios.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infra
builder.Services.AddUsuariosInfrastructure(builder.Configuration);

// JSON camelCase
builder.Services.AddControllers()
	.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// CORS para Angular (https dev)
builder.Services.AddCors(opt =>
{
	opt.AddPolicy("AllowAngularDev", p =>
		p.WithOrigins("https://localhost:4443", "http://localhost:4200") // por si corres http
		 .AllowAnyHeader()
		 .AllowAnyMethod());
});

// JWT
builder.Services.Configure<Usuarios.Infrastructure.Auth.JwtSettings>(builder.Configuration.GetSection("Jwt"));
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(o =>
	{
		o.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ClockSkew = TimeSpan.Zero
		};
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
