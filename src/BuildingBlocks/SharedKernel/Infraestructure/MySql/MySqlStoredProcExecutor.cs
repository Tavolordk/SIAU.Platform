#nullable enable
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySqlConnector;
using SharedKernel.Abstractions;

namespace Solicitudes.Infrastructure.Data
{
	public sealed class MySqlQueryExecutor : IQueryExecutor
	{
		private readonly IConnectionFactory _factory;

		public MySqlQueryExecutor(IConnectionFactory factory) => _factory = factory;

		public IDbDataParameter CreateParameter(string name, object? value, DbType dbType)
		{
			var p = new MySqlParameter(Normalize(name), value ?? DBNull.Value);
			p.DbType = dbType;
			return p;
		}

		public async Task<int> ExecuteCountAsync(
			string sql,
			IReadOnlyDictionary<string, object?> parameters,
			CancellationToken ct = default)
		{
			await using var conn = (MySqlConnection)_factory.Create();
			await conn.OpenAsync(ct);

			await using var cmd = new MySqlCommand(sql, conn) { CommandType = CommandType.Text };
			AddParams(cmd, parameters);
			var obj = await cmd.ExecuteScalarAsync(ct);
			return obj is null or DBNull ? 0 : Convert.ToInt32(obj);
		}

		public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
			string sql,
			IReadOnlyDictionary<string, object?> parameters,
			CancellationToken ct = default)
		{
			await using var conn = (MySqlConnection)_factory.Create();
			await conn.OpenAsync(ct);

			await using var cmd = new MySqlCommand(sql, conn) { CommandType = CommandType.Text };
			AddParams(cmd, parameters);

			var list = new List<IReadOnlyDictionary<string, object?>>();
			await using var r = await cmd.ExecuteReaderAsync(ct);
			while (await r.ReadAsync(ct))
			{
				var row = new Dictionary<string, object?>(r.FieldCount, StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < r.FieldCount; i++)
					row[r.GetName(i)] = await r.IsDBNullAsync(i, ct) ? null : r.GetValue(i);
				list.Add(row);
			}
			return list;
		}

		private static void AddParams(MySqlCommand cmd, IReadOnlyDictionary<string, object?> parameters)
		{
			foreach (var (k, v) in parameters)
				cmd.Parameters.AddWithValue(Normalize(k), v ?? DBNull.Value);
		}

		private static string Normalize(string name) =>
			string.IsNullOrEmpty(name) ? name : (name[0] == '@' ? name : "@" + name);
	}
}
