namespace SharedKernel.Abstractions;

public interface IQueryExecutor
{
	Task<int> ExecuteCountAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct);
	Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct);
}
