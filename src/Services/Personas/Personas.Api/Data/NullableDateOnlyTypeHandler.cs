using Dapper;
using System.Data;

public sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
	public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
		parameter.Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;

	public override DateOnly? Parse(object value) =>
		value is null or DBNull ? null :
		value is DateTime dt ? DateOnly.FromDateTime(dt) : DateOnly.Parse(value.ToString()!);
}