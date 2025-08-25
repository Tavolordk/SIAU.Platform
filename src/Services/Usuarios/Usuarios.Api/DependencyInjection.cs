using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Usuarios.Application.Interfaces;
using Usuarios.Application.Services;
using Usuarios.Infrastructure.Persistence;
using Usuarios.Infrastructure.Repositories;

namespace Usuarios.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddUsuariosInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<UsuariosDbContext>(options =>
				options.UseMySql(configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(configuration.GetConnectionString("MySql"))));

			services.AddScoped<IUsuarioRepository, UsuarioRepository>();
			services.AddScoped<IUsuarioService, UsuarioService>();

			return services;
		}
	}
}
