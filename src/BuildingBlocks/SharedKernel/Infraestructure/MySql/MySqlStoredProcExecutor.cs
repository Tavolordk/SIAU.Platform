#nullable enable
using MySqlConnector;
using SharedKernel.Data;   // << asegúrate que esta es la interfaz que usas en handlers
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Solicitudes.Infrastructure.Data
{
	public sealed class MySqlStoredProcExecutor : IStoredProcExecutor
	{
		private readonly string _connectionString;

		public MySqlStoredProcExecutor(string connectionString)
			=> _connectionString = connectionString;

		public IDbDataParameter CreateParameter(
			string name, object? value, DbType dbType,
			ParameterDirection direction = ParameterDirection.Input)
		{
			var p = new MySqlParameter(
				name.Length > 0 && name[0] == '@' ? name : "@" + name,
				value ?? DBNull.Value)
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
			await using var conn = new MySqlConnection(_connectionString);
			await conn.OpenAsync(ct);

			await using var cmd = new MySqlCommand(procName, conn)
			{ CommandType = CommandType.StoredProcedure };

			foreach (var p in parameters) cmd.Parameters.Add(p);
			return await cmd.ExecuteNonQueryAsync(ct);
		}

		public async Task<IDataReader> ExecuteReaderAsync(
			string procName,
			IEnumerable<IDbDataParameter> parameters,
			CancellationToken ct = default)
		{
			var conn = new MySqlConnection(_connectionString);
			await conn.OpenAsync(ct);

			var cmd = new MySqlCommand(procName, conn)
			{ CommandType = CommandType.StoredProcedure };

			foreach (var p in parameters) cmd.Parameters.Add(p);

			// el reader cerrará la conexión al cerrarse
			return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
		}
	}
}
