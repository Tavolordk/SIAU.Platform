using System.Threading.Tasks;
using Usuarios.Api.Dtos;
using Usuarios.Domain.Entities;

namespace Usuarios.Application.Interfaces
{
	public interface IUsuarioService
	{
		Task<LoginResponse?> LoginAsync(string cuenta, string password, CancellationToken ct = default);
		Task<bool> CambiarPasswordAsync(string cuenta, string passwordActual, string nuevaPassword);
		Task<Usuario?> GetByIdAsync(int id);
	}
}
