using System.Data;
using MySqlConnector;
using SharedKernel.Data;

namespace Solicitudes.Infrastructure.Data
{
	public class MySqlStoredProcExecutor : IStoredProcExecutor
	{
		private readonly string _connString;
		public MySqlStoredProcExecutor(string connString) => _connString = connString;

		public IDbDataParameter CreateParameter(string name, object? value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
		{
			var p = new MySqlParameter(name, value ?? DBNull.Value) { Direction = direction };
			p.DbType = dbType;
			return p;
		}

		public async Task<int> ExecuteAsync(string procName, IEnumerable<IDbDataParameter> parameters)
		{
			await using var conn = new MySqlConnection(_connString);
			await conn.OpenAsync();
			await using var cmd = new MySqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure };
			foreach (var p in parameters) cmd.Parameters.Add(p);
			return await cmd.ExecuteNonQueryAsync();
		}

		public async Task<IDataReader> ExecuteReaderAsync(string procName, IEnumerable<IDbDataParameter> parameters)
		{
			var conn = new MySqlConnection(_connString);
			await conn.OpenAsync();
			var cmd = new MySqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure };
			foreach (var p in parameters) cmd.Parameters.Add(p);
			return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
		}
	}
}
