namespace Documentos.Api.Storage;

public sealed class DocumentosOptions
{
	public string Proveedor { get; set; } = "LOCAL";
	public string? LocalRoot { get; set; }
}
