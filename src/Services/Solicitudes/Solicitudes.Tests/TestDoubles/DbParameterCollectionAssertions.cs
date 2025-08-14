using System.Data;

namespace TestDoubles;

public static class DbParameterCollectionAssertions
{
	public static object? ValueOf(this IDataParameterCollection parameters, string name)
		=> ((IDataParameter)parameters[name]).Value;

	public static T? ValueOf<T>(this IDataParameterCollection parameters, string name)
	{
		var val = ((IDataParameter)parameters[name]).Value;
		return val is null ? default : (T)Convert.ChangeType(val, typeof(T))!;
	}
}
