using System.Data;

namespace TestDoubles;

public sealed class FakeDbCommand : IDbCommand
{
	private readonly FakeDbParameterCollection _parameters = new();

	public string CommandText { get; set; } = string.Empty;
	public int CommandTimeout { get; set; } = 30;
	public CommandType CommandType { get; set; } = CommandType.Text;

	public IDbConnection? Connection { get; set; }
	public IDataParameterCollection Parameters => _parameters;
	public IDbTransaction? Transaction { get; set; }
	public UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;

	public void Cancel() { /* no-op */ }
	public IDbDataParameter CreateParameter() => new FakeDbParameter();
	public void Dispose() { Disposed?.Invoke(this, EventArgs.Empty); }
	public int ExecuteNonQuery() => AffectedRows;
	public IDataReader ExecuteReader() => throw new NotSupportedException("Use ExecuteNonQuery in unit tests.");
	public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotSupportedException();
	public object? ExecuteScalar() => ScalarResult;
	public void Prepare() { /* no-op */ }

	// Configurables en test
	public int AffectedRows { get; set; } = 1;
	public object? ScalarResult { get; set; }

	// Evento útil para leer outputs después de 'ejecución simulada'
	public event EventHandler? Disposed;

	// Helpers de prueba
	public FakeDbParameterCollection AsFakeParameters() => _parameters;
}
