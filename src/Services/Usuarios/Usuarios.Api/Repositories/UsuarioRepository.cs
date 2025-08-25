using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using MySqlConnector;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Entities;
using Usuarios.Infrastructure.Persistence;

namespace Usuarios.Infrastructure.Repositories
{
	public class UsuarioRepository : IUsuarioRepository
	{
		private readonly UsuariosDbContext _context;
		private readonly string _cs;
		public UsuarioRepository(UsuariosDbContext context, IConfiguration cfg)
		{
			_context = context;
			_cs = cfg.GetConnectionString("MySql")
	 ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql");
		}
		public async Task<(int, string, string, string, string)?> FindForLoginAsync(
			string cuenta, string password, CancellationToken ct = default)
		{
			var hash = ComputeSha256(password);

			const string sql = @"
SELECT 
    u.id                                        AS UserId,
    u.cuenta                                    AS Cuenta,
    COALESCE(tu.descripcion,'USER')                  AS Rol,
    TRIM(CONCAT_WS(' ', p.nombres, p.primer_apellido, p.segundo_apellido)) AS NombreCompleto,
    COALESCE(est.nombre,'')                     AS NombreEstado
FROM usuarios u
JOIN persona p                 ON p.id = u.persona_id
LEFT JOIN cat_tp_usuarios tu   ON tu.id = u.tipo_usuario_id
LEFT JOIN cat_division_geopolitica est ON est.id = p.entidad_nacimiento_id
WHERE u.cuenta = @cuenta
  AND u.password_hash = @hash
  AND u.activo = 1
LIMIT 1;";

			await using var conn = new MySqlConnection(_cs);
			await conn.OpenAsync(ct);

			var row = await conn.QuerySingleOrDefaultAsync<UserLoginRow>(
				new CommandDefinition(sql, new { cuenta, hash }, cancellationToken: ct));

			if (row is null) return null;

			static string Clean(string s) =>
				string.Join(' ', (s ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));

			return (row.UserId, row.Cuenta, row.Rol, Clean(row.NombreCompleto), row.NombreEstado ?? "");
		}

		// Clase de mapeo para Dapper (privada al repositorio)
		private sealed class UserLoginRow
		{
			public int UserId { get; set; }
			public string Cuenta { get; set; } = "";
			public string Rol { get; set; } = "USER";
			public string NombreCompleto { get; set; } = "";
			public string NombreEstado { get; set; } = "";
		}


		public async Task<Usuario?> LoginAsync(string cuenta, string password)
		{
			var hash = ComputeSha256(password);

			return await _context.Usuarios
				.AsNoTracking()
				.FirstOrDefaultAsync(u =>
					u.Cuenta == cuenta &&
					u.Password_hash == hash &&
					u.Activo);
		}

		public async Task<bool> CambiarPasswordAsync(string cuenta, string passwordActual, string nuevaPassword)
		{
			var hashActual = ComputeSha256(passwordActual);

			var user = await _context.Usuarios
				.FirstOrDefaultAsync(u =>
					u.Cuenta == cuenta &&
					u.Password_hash == hashActual &&
					u.Activo);

			if (user is null) return false;

			user.Password_hash = ComputeSha256(nuevaPassword);
			// Si tienes campo de fecha de mod: user.Fecha_modificacion = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<Usuario?> GetByIdAsync(int id)
		{
			return await _context.Usuarios
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		private static string ComputeSha256(string raw)
		{
			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
			var sb = new StringBuilder();
			foreach (var b in bytes) sb.Append(b.ToString("x2"));
			return sb.ToString();
		}
	}
}
