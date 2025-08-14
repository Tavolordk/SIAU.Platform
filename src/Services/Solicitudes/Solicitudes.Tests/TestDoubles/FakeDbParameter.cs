#nullable enable
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace TestDoubles;

public sealed class FakeDbParameter : IDbDataParameter
{
	public FakeDbParameter() { }
	public FakeDbParameter(string? name, object? value = null)
	{
		ParameterName = name ?? string.Empty;
		Value = value;
	}

	// IDbDataParameter
	public byte Precision { get; set; }
	public byte Scale { get; set; }
	public int Size { get; set; }

	// IDataParameter
	public DbType DbType { get; set; } = DbType.Object;
	public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
	public bool IsNullable => true;

	// ==> Mantén string (no anulable) y permite null en el setter
	public string ParameterName { get; set; } = string.Empty;
	public string SourceColumn { get; set; } = string.Empty;

	public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;
	public object? Value { get; set; }
}
