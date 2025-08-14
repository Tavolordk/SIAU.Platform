#nullable enable
using System.Data;
using System.Diagnostics.CodeAnalysis;
using MySqlConnector;
using SharedKernel.Data; // <-- mismo namespace de la interfaz

namespace Solicitudes.Infrastructure.Data
{
	[ExcludeFromCodeCoverage]
	public sealed class MySqlStoredProcExecutor : IStoredProcExecutor
	{
		private readonly string _connString;
		public MySqlStoredProcExecutor(string connString) => _connString = connString;

		public IDbDataParameter CreateParameter(
			string name,
			object? value,
			DbType dbType,
			ParameterDirection direction = ParameterDirection.Input)
		{
			// Normaliza el nombre para aceptar "p_x" o "@p_x"
			var paramName = string.IsNullOrEmpty(name) ? name : (name[0] == '@' ? name : "@" + name);
			var p = new MySqlParameter(paramName, value ?? DBNull.Value)
			{
				DbType = dbType,
				Direction = direction
			};
			return p;
		}

		public async Task<int> ExecuteAsync(
			string procName,
			IEnumerable<IDbDataParameter> parameters,
			CancellationToken ct = default)
		{
			await using var conn = new MySqlConnection(_connString);
			await conn.OpenAsync(ct);

			await using var cmd = new MySqlCommand(procName, conn)
			{
				CommandType = CommandType.StoredProcedure
			};

			foreach (var p in parameters) cmd.Parameters.Add(p);

			return await cmd.ExecuteNonQueryAsync(ct);
		}

		public async Task<IDataReader> ExecuteReaderAsync(
			string procName,
			IEnumerable<IDbDataParameter> parameters,
			CancellationToken ct = default)
		{
			var conn = new MySqlConnection(_connString);
			await conn.OpenAsync(ct);

			var cmd = new MySqlCommand(procName, conn)
			{
				CommandType = CommandType.StoredProcedure
			};

			foreach (var p in parameters) cmd.Parameters.Add(p);

			// Cierra la conexión cuando cierres el reader
			return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
		}
	}
}
