using System.Data;
using System.Data.Common;
using System.Collections.ObjectModel; // ReadOnlyDictionary
using Dapper;
using SharedKernel.Abstractions;
using SharedKernel.Data;

namespace Solicitudes.Infrastructure.Data;

public sealed class MySqlQueryExecutor(IConnectionFactory factory) : IQueryExecutor
{
	// 1) Conteo
	public async Task<int> ExecuteCountAsync(
		string sql,
		IReadOnlyDictionary<string, object?> parameters,
		CancellationToken ct)
	{
		using var conn = factory.Create(); // IDbConnection (p.ej., MySqlConnection)
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		return await conn.ExecuteScalarAsync<int>(
			new CommandDefinition(sql, new DynamicParameters(parameters), cancellationToken: ct));
	}

	// 2) Query “diccionario” (si tu kernel la usa en otros sitios)
	public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
		string sql,
		IReadOnlyDictionary<string, object?> parameters,
		CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		var rows = await conn.QueryAsync(
			new CommandDefinition(sql, new DynamicParameters(parameters), cancellationToken: ct));

		var list = new List<IReadOnlyDictionary<string, object?>>();
		foreach (var row in rows)
		{
			var src = (IDictionary<string, object>)row; // DapperRow
			var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach (var kv in src) dict[kv.Key] = kv.Value;
			list.Add(new ReadOnlyDictionary<string, object?>(dict));
		}
		return list;
	}

	// 3) NUEVOS genéricos (lo que llamas desde el controller)
	public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		return await conn.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
	}

	public async Task<T> QuerySingleAsync<T>(string sql, object? param = null, CancellationToken ct = default)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();

		return await conn.QuerySingleAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
	}

	// 4) (OPCIONAL) Multi-result: si decides usarlo, NO cierres la conexión aquí;
	//    el GridReader se encargará al ser disposed por el consumidor.
	
    public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        var conn = (DbConnection)factory.Create();
        await conn.OpenAsync(ct);
        return await conn.QueryMultipleAsync(new CommandDefinition(sql, param, cancellationToken: ct));
    }
}
