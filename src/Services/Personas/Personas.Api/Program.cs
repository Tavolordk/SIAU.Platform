using Dapper;
using Personas.Api.Data;
using SharedKernel.Abstractions;
using SharedKernel.Infrastructure.MySql; // <- importante

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("MySql")
		 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql");

// DI
builder.Services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(cs));
builder.Services.AddScoped<IPersonasRepository, PersonasRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();   // <- necesario para swagger
builder.Services.AddSwaggerGen();             // <- necesario para swagger

var app = builder.Build();

app.UseSwagger();                             // <- necesario para swagger
app.UseSwaggerUI();                           // <- necesario para swagger
app.MapControllers();
app.Run();

// Para WebApplicationFactory en tests
public partial class Program { }
