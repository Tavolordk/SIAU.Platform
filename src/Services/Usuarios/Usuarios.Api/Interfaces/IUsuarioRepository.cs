using Usuarios.Domain.Entities;

namespace Usuarios.Application.Interfaces
{
	public interface IUsuarioRepository
	{
		Task<Usuario?> LoginAsync(string cuenta, string password);
		Task<bool> CambiarPasswordAsync(string cuenta, string passwordActual, string nuevaPassword);
		Task<Usuario?> GetByIdAsync(int id);
		Task<(int UserId, string Cuenta, string Rol, string NombreCompleto, string NombreEstado)?>
	FindForLoginAsync(string cuenta, string password, CancellationToken ct = default);
	}
}
