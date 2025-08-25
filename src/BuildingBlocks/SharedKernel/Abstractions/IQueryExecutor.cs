using Dapper;

namespace SharedKernel.Abstractions;

public interface IQueryExecutor
{
	Task<int> ExecuteCountAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct);
	Task<T> QuerySingleAsync<T>(string sql, object? param = null, CancellationToken ct = default);
	Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default);

	Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct);
	Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, CancellationToken ct = default);

}
