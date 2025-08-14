#nullable enable
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace TestDoubles;

public sealed class FakeDbCommand : IDbCommand
{
	private readonly FakeDbParameterCollection _parameters = new();

	// <- string (no null) + AllowNull en el setter; valor por defecto = ""
	private string _commandText = string.Empty;
	[AllowNull]
	public string CommandText
	{
		get => _commandText;
		set => _commandText = value ?? string.Empty;
	}
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
	public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotSupportedException("Use ExecuteNonQuery in unit tests.");
	public object? ExecuteScalar() => ScalarResult;
	public void Prepare() { /* no-op */ }

	// Configurables en test
	public int AffectedRows { get; set; } = 1;
	public object? ScalarResult { get; set; }

	public event EventHandler? Disposed;

	public FakeDbParameterCollection AsFakeParameters() => _parameters;
}
