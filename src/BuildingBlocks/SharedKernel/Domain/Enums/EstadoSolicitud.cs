namespace SharedKernel.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public sealed class EstadoSolicitud
{
	private static readonly Dictionary<string, EstadoSolicitud> _by = new(StringComparer.OrdinalIgnoreCase);
	public static readonly EstadoSolicitud Creada = new("CREADA");
	public static readonly EstadoSolicitud EnRevision = new("EN_REVISION");
	public static readonly EstadoSolicitud Aprobada = new("APROBADA");
	public static readonly EstadoSolicitud Rechazada = new("RECHAZADA");

	public string Key { get; }
	private EstadoSolicitud(string key) { Key = key; _by[key] = this; }
	public static EstadoSolicitud FromKey(string key) =>
		_by.TryGetValue(key, out var v) ? v : throw new ArgumentException($"Estado desconocido: {key}");
	public override string ToString() => Key;
}
