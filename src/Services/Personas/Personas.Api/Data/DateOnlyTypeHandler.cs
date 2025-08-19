using Dapper;
using System.Data;

public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
	public override void SetValue(IDbDataParameter parameter, DateOnly value) =>
		parameter.Value = value.ToDateTime(TimeOnly.MinValue);

	public override DateOnly Parse(object value) =>
		value switch
		{
			DateTime dt => DateOnly.FromDateTime(dt),
			string s => DateOnly.Parse(s),
			_ => default
		};
}