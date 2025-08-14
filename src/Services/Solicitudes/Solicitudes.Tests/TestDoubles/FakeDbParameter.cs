#nullable enable
using System.Data;

namespace TestDoubles;

public sealed class FakeDbParameter : IDbDataParameter
{
	public FakeDbParameter() { }

	public FakeDbParameter(string? name, object? value = null)
	{
		ParameterName = name;
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

	public string? ParameterName { get; set; }   // <- string?
	public string? SourceColumn { get; set; }   // <- string?
	public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;
	public object? Value { get; set; }
}
