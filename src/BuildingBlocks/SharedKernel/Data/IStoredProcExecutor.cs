using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SharedKernel.Data
{
	public interface IStoredProcExecutor
	{
		Task<int> ExecuteAsync(string procName, IEnumerable<IDbDataParameter> parameters);
		Task<IDataReader> ExecuteReaderAsync(string procName, IEnumerable<IDbDataParameter> parameters);
		IDbDataParameter CreateParameter(string name, object? value, DbType dbType, ParameterDirection direction = ParameterDirection.Input);
	}
}
