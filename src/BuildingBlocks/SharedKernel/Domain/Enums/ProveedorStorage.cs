namespace SharedKernel.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public sealed class ProveedorStorage
{
	private static readonly Dictionary<string, ProveedorStorage> _by = new(StringComparer.OrdinalIgnoreCase);
	public static readonly ProveedorStorage S3 = new("S3");
	public static readonly ProveedorStorage Gcs = new("GCS");
	public static readonly ProveedorStorage Azure = new("AZURE");
	public static readonly ProveedorStorage Local = new("LOCAL");
	public static readonly ProveedorStorage Otro = new("OTRO");

	public string Key { get; }
	private ProveedorStorage(string key) { Key = key; _by[key] = this; }
	public static ProveedorStorage FromKey(string key) =>
		_by.TryGetValue(key, out var v) ? v : throw new ArgumentException($"Proveedor desconocido: {key}");
	public override string ToString() => Key;
}
