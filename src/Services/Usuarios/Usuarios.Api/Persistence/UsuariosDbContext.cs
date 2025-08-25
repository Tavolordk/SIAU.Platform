using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Usuarios.Domain.Entities;

namespace Usuarios.Infrastructure.Persistence
{
	public class UsuariosDbContext : DbContext
	{
		public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }

		public DbSet<Usuario> Usuarios { get; set; }
	}
}
