namespace Usuarios.Api.Dtos
{
	public class LoginRequest
	{
		// Tu front a veces manda { cuenta, contrasena } y a veces { username, password }.
		// Aceptamos ambos para máxima compatibilidad.
		public string? Cuenta { get; set; }
		public string? Contrasena { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }
	}

	public class LoginResponse
	{
		public string Token { get; set; } = string.Empty;
		public bool PrimerIngreso { get; set; } = false;
		public string NombreCompleto { get; set; } = string.Empty;
		public string Rol { get; set; } = "USER";
		public string NombreEstado { get; set; } = string.Empty;
		public int UserId { get; set; }
		public string? RefreshToken { get; set; }
	}

	public class PrimerInicioRequest
	{
		public string Cuenta { get; set; } = string.Empty;
		public string NuevaPassword { get; set; } = string.Empty;
	}

	public class CambiarPasswordRequest
	{
		public string Cuenta { get; set; } = string.Empty;
		public string PasswordActual { get; set; } = string.Empty;
		public string NuevaPassword { get; set; } = string.Empty;
	}

	public class OlvidoPasswordRequest
	{
		public string Correo { get; set; } = string.Empty;
	}
}
