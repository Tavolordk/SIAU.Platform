using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Usuarios.Api.Dtos;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Entities;
using Usuarios.Infrastructure.Auth;

namespace Usuarios.Api.Controllers
{
	[ApiController]
	// Soportamos AMBAS rutas: /user/* y /api/user/* para no tocar tu front
	[Route("user")]
	[Route("api/user")]
	public class UserController : ControllerBase
	{
		private readonly IUsuarioService _usuarioService;
		private readonly JwtSettings _jwt;

		public UserController(IUsuarioService usuarioService, IOptions<JwtSettings> jwtOptions)
		{
			_usuarioService = usuarioService;
			_jwt = jwtOptions.Value;
		}

		// POST /user/login  (o /api/user/login)
		// UserController.cs
		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
		{
			var cuenta = !string.IsNullOrWhiteSpace(req.Cuenta) ? req.Cuenta : (req.Username ?? "");
			var pass = !string.IsNullOrWhiteSpace(req.Contrasena) ? req.Contrasena : (req.Password ?? "");

			var result = await _usuarioService.LoginAsync(cuenta, pass, ct);
			return result is null ? Unauthorized(new { message = "Credenciales inválidas" }) : Ok(result);
		}


		// PUT /user/primer-inicio
		[AllowAnonymous]
		[HttpPut("primer-inicio")]
		public async Task<IActionResult> PrimerInicio([FromBody] PrimerInicioRequest req)
		{
			// Reutilizamos la misma lógica de cambiar password, pero SIN exigir password actual
			var ok = await _usuarioService.CambiarPasswordAsync(req.Cuenta, passwordActual: null!, req.NuevaPassword);
			if (!ok) return BadRequest(new { message = "No se pudo cambiar la contraseña de primer inicio." });
			return NoContent();
		}

		// PUT /user/cambiar-password (requiere JWT si quieres)
		[Authorize]
		[HttpPut("cambiar-password")]
		public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest req)
		{
			var ok = await _usuarioService.CambiarPasswordAsync(req.Cuenta, req.PasswordActual, req.NuevaPassword);
			if (!ok) return Unauthorized(new { message = "La contraseña actual no es correcta." });
			return NoContent();
		}

		// POST /user/olvido-password
		[AllowAnonymous]
		[HttpPost("olvido-password")]
		public IActionResult OlvidoPassword([FromBody] OlvidoPasswordRequest _)
		{
			// Por seguridad no reveles si el correo existe o no
			// (Cuando quieras, integramos token de reset + envío de correo)
			return Ok(new { ok = true });
		}
	}
}
