using System.Data;
using Dapper;
using MySqlConnector;

namespace SharedKernel.Data;

// DateOnly -> DATE
public sealed class MySqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
	public override void SetValue(IDbDataParameter parameter, DateOnly value)
	{
		if (parameter is MySqlParameter mp) mp.MySqlDbType = MySqlDbType.Date;
		parameter.Value = value.ToDateTime(TimeOnly.MinValue);
	}

	public override DateOnly Parse(object value) => value switch
	{
		DateOnly d => d,
		DateTime dt => DateOnly.FromDateTime(dt),
		string s => DateOnly.Parse(s),
		_ => throw new DataException($"No puedo convertir {value?.GetType().FullName} a DateOnly")
	};
}

// (opcional) TimeOnly -> TIME (por si luego lo usas)
public sealed class MySqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
	public override void SetValue(IDbDataParameter parameter, TimeOnly value)
	{
		if (parameter is MySqlParameter mp) mp.MySqlDbType = MySqlDbType.Time;
		parameter.Value = value.ToTimeSpan();
	}

	public override TimeOnly Parse(object value) => value switch
	{
		TimeOnly t => t,
		TimeSpan ts => TimeOnly.FromTimeSpan(ts),
		string s => TimeOnly.Parse(s),
		_ => throw new DataException($"No puedo convertir {value?.GetType().FullName} a TimeOnly")
	};
}
