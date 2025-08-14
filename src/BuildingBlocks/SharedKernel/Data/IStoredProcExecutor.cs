using System.Data;

namespace SharedKernel.Data
{
	public interface IStoredProcExecutor
	{
		Task<int> ExecuteAsync(
			string procName,
			IEnumerable<IDbDataParameter> parameters,
			CancellationToken ct = default);

		Task<IDataReader> ExecuteReaderAsync(
			string procName,
			IEnumerable<IDbDataParameter> parameters,
			CancellationToken ct = default);

		IDbDataParameter CreateParameter(
			string name,
			object? value,
			DbType dbType,
			ParameterDirection direction = ParameterDirection.Input);
	}
}
