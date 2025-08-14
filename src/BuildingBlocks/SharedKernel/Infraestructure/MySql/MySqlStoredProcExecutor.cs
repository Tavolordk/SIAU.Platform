#nullable enable
using System.Data;
using MySql.Data.MySqlClient;

namespace SharedKernel.Infrastructure.MySql;
using SharedKernel.Abstractions;

public sealed class MySqlQueryExecutor(IConnectionFactory factory) : IQueryExecutor
{
	public async Task<int> ExecuteCountAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct)
	{
		using var conn = (MySqlConnection)factory.Create();
		await conn.OpenAsync(ct);
		using var cmd = new MySqlCommand(sql, conn);
		foreach (var kv in parameters) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
		var obj = await cmd.ExecuteScalarAsync(ct);
		return Convert.ToInt32(obj);
	}

	public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct)
	{
		using var conn = (MySqlConnection)factory.Create();
		await conn.OpenAsync(ct);
		using var cmd = new MySqlCommand(sql, conn);
		foreach (var kv in parameters) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);

		var list = new List<IReadOnlyDictionary<string, object?>>();
		using var r = await cmd.ExecuteReaderAsync(ct);
		while (await r.ReadAsync(ct))
		{
			var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < r.FieldCount; i++)
				row[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
			list.Add(row);
		}
		return list;
	}
}
