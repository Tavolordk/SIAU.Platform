using System.Data;
using System.Data.Common;
using System.Collections.ObjectModel; // ReadOnlyDictionary
using Dapper;
using SharedKernel.Abstractions;

namespace Solicitudes.Infrastructure.Data;

public sealed class MySqlQueryExecutor(IConnectionFactory factory) : IQueryExecutor
{
	// 1) Conteo
	public async Task<int> ExecuteCountAsync(
		string sql,
		IReadOnlyDictionary<string, object?> parameters,
		CancellationToken ct)
	{
		using var conn = factory.Create(); // IDbConnection (MySqlConnector)
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		return await conn.ExecuteScalarAsync<int>(sql, new DynamicParameters(parameters));
	}

	// 2) Query “simple” que exige la interfaz:
	//    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ...
	public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
		string sql,
		IReadOnlyDictionary<string, object?> parameters,
		CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		var rows = await conn.QueryAsync(sql, new DynamicParameters(parameters)); // IEnumerable<dynamic>

		var list = new List<IReadOnlyDictionary<string, object?>>();
		foreach (var row in rows)
		{
			// DapperRow implementa IDictionary<string, object>
			var src = (IDictionary<string, object>)row;

			// Copiamos a un diccionario <string, object?> y lo exponemos como ReadOnly
			var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach (var kv in src)
				dict[kv.Key] = kv.Value;

			list.Add(new ReadOnlyDictionary<string, object?>(dict));
		}

		return list;
	}

	// 3) (Opcional) helper genérico con map: no lo exige la interfaz,
	//    mantenlo solo si lo usas en tu código.
	public async Task<IReadOnlyList<T>> QueryAsync<T>(
		string sql,
		Func<IDataReader, T> map,
		IReadOnlyDictionary<string, object?> parameters,
		CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		foreach (var kv in parameters)
		{
			var p = cmd.CreateParameter();
			p.ParameterName = kv.Key;
			p.Value = kv.Value ?? DBNull.Value;
			cmd.Parameters.Add(p);
		}

		using var rdr = await ((DbCommand)cmd).ExecuteReaderAsync(ct);
		var list = new List<T>();
		while (await rdr.ReadAsync(ct))
			list.Add(map(rdr));
		return list;
	}
}
