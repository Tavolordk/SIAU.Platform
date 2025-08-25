namespace Usuarios.Domain.Entities
{
	public class Usuario
	{
		public int Id { get; set; }
		public string Cuenta { get; set; } = string.Empty;
		public string Password_hash { get; set; } = string.Empty;
		public bool Activo { get; set; } = true;
	}
}
