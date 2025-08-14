using System.Collections;
using System.Data;

namespace TestDoubles;

public sealed class FakeDbParameterCollection : IDataParameterCollection
{
	private readonly List<IDataParameter> _list = new();

	public object this[int index]
	{
		get => _list[index];
		set => _list[index] = (IDataParameter)value;
	}

	public object this[string parameterName]
	{
		get
		{
			var i = IndexOf(parameterName);
			if (i < 0) throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
			return _list[i];
		}
		set
		{
			var i = IndexOf(parameterName);
			if (i < 0) _list.Add((IDataParameter)value);
			else _list[i] = (IDataParameter)value;
		}
	}

	public int Count => _list.Count;
	public bool IsFixedSize => false;
	public bool IsReadOnly => false;
	public bool IsSynchronized => false;
	public object SyncRoot => this;

	public int Add(object value)
	{
		_list.Add((IDataParameter)value);
		return _list.Count - 1;
	}

	public void Clear() => _list.Clear();

	public bool Contains(object value) => _list.Contains((IDataParameter)value);

	public bool Contains(string parameterName) => IndexOf(parameterName) >= 0;

	public void CopyTo(Array array, int index) => ((ICollection)_list).CopyTo(array, index);

	public IEnumerator GetEnumerator() => _list.GetEnumerator();

	public int IndexOf(object value) => _list.IndexOf((IDataParameter)value);

	public int IndexOf(string parameterName)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (string.Equals(_list[i].ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
				return i;
		}
		return -1;
	}

	public void Insert(int index, object value) => _list.Insert(index, (IDataParameter)value);

	public void Remove(object value) => _list.Remove((IDataParameter)value);

	public void RemoveAt(int index) => _list.RemoveAt(index);

	public void RemoveAt(string parameterName)
	{
		var i = IndexOf(parameterName);
		if (i >= 0) _list.RemoveAt(i);
	}

	// Helpers de prueba
	public T? GetValue<T>(string name)
	{
		var p = (IDataParameter)this[name];
		return p.Value is null ? default : (T)Convert.ChangeType(p.Value, typeof(T))!;
	}
}
