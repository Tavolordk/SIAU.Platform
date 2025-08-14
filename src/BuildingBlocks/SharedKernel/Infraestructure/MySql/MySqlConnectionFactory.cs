using MySql.Data.MySqlClient;
using SharedKernel.Abstractions;
using System.Data;

namespace SharedKernel.Infrastructure.MySql;
public sealed class MySqlConnectionFactory(string cs) : IConnectionFactory
{
	public IDbConnection Create() => new MySqlConnection(cs);
}
