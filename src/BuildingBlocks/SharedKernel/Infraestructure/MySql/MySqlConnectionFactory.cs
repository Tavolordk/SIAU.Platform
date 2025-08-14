using MySql.Data.MySqlClient;
using SharedKernel.Abstractions;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace SharedKernel.Infrastructure.MySql;
[ExcludeFromCodeCoverage]
public sealed class MySqlConnectionFactory(string cs) : IConnectionFactory
{
	public IDbConnection Create() => new MySqlConnection(cs);
}
