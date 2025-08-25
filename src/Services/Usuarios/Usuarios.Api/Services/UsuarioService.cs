using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Usuarios.Api.Dtos;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Entities;
using Usuarios.Infrastructure.Auth;

namespace Usuarios.Application.Services
{
	public class UsuarioService : IUsuarioService
	{
		private readonly IUsuarioRepository _repository;
		private readonly JwtSettings _jwt;

		public UsuarioService(IUsuarioRepository repository, IOptions<JwtSettings> jwtOptions)
		{
			_repository = repository;
			_jwt = jwtOptions.Value;
		}

		public async Task<LoginResponse?> LoginAsync(string cuenta, string password, CancellationToken ct = default)
		{
			var row = await _repository.FindForLoginAsync(cuenta, password, ct);
			if (row is null) return null;

			var (userId, acc, rol, nombreCompleto, nombreEstado) = row.Value;
			var token = GenerateJwt(userId, acc, rol);

			return new LoginResponse
			{
				Token = token,
				PrimerIngreso = false,
				NombreCompleto = nombreCompleto ?? string.Empty,
				Rol = string.IsNullOrWhiteSpace(rol) ? "USER" : rol,
				NombreEstado = nombreEstado ?? string.Empty,
				UserId = userId,
				RefreshToken = null
			};
		}

		public Task<bool> CambiarPasswordAsync(string cuenta, string passwordActual, string nuevaPassword)
			=> _repository.CambiarPasswordAsync(cuenta, passwordActual, nuevaPassword);

		public Task<Usuario?> GetByIdAsync(int id)
			=> _repository.GetByIdAsync(id);

		private string GenerateJwt(int userId, string cuenta, string rol)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, cuenta),
				new Claim("id", userId.ToString()),
				new Claim(ClaimTypes.Role, rol)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var jwt = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(jwt);
		}
	}
}
