using System.Data;

namespace SharedKernel.Abstractions;

public sealed record SpResult(int Rows, IReadOnlyDictionary<string, object?> Outputs);

public interface IStoredProcExecutor
{
	Task<SpResult> ExecAsync(
		string procName,
		Action<IDbCommand> configure,
		CancellationToken ct);
}
