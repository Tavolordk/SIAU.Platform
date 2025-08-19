namespace Documentos.Api.Storage;

public sealed class StorageSaveResult(
	string rutaRelativa,
	string mimeType,
	string extension,
	int tamanoBytes,
	string checksumSha256)
{
	public string RutaRelativa { get; } = rutaRelativa;
	public string MimeType { get; } = mimeType;
	public string Extension { get; } = extension;
	public int TamanoBytes { get; } = tamanoBytes;
	public string ChecksumSha256 { get; } = checksumSha256;
}

public interface IStorageProvider
{
	Task<StorageSaveResult> SaveAsync(
		IFormFile file,
		uint solicitudId,
		byte tipoDocumentoId,
		CancellationToken ct);

	Task<Stream> OpenReadAsync(string rutaRelativa, CancellationToken ct);

	Task DeleteAsync(string rutaRelativa, CancellationToken ct);
}
