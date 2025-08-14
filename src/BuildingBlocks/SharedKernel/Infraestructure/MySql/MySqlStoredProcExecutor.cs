using System.Data;
using MySql.Data.MySqlClient;
using SharedKernel.Abstractions;

namespace SharedKernel.Infrastructure.MySql;

public sealed class MySqlStoredProcExecutor(IConnectionFactory factory) : IStoredProcExecutor
{
	public async Task<SpResult> ExecAsync(string procName, Action<IDbCommand> configure, CancellationToken ct)
	{
		using var conn = (MySqlConnection)factory.Create();
		await conn.OpenAsync(ct);

		using var cmd = new MySqlCommand(procName, conn)
		{
			CommandType = CommandType.StoredProcedure
		};

		configure(cmd);

		var rows = await cmd.ExecuteNonQueryAsync(ct);

		// Recoger outputs
		var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
		foreach (MySqlParameter p in cmd.Parameters)
		{
			if (p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput or ParameterDirection.ReturnValue)
			{
				outputs[p.ParameterName] = p.Value;
			}
		}

		return new SpResult(rows, outputs);
	}
}
