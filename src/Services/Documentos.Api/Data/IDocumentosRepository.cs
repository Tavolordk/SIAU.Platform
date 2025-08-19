// Documentos.Api/Data/IDocumentosRepository.cs
using Documentos.Api.Models;

namespace Documentos.Api.Data;

public interface IDocumentosRepository
{
	Task<uint> UpsertDocumentoAsync(
		uint solicitudId,
		byte tipoDocumentoId,
		string nombreOriginal,
		string? extension,
		string? mimeType,
		int tamanoBytes,
		string storageProveedor,
		string storageRuta,
		string checksumSha256,
		uint usuarioId,
		CancellationToken ct);

	Task<IReadOnlyList<DocumentoItemDto>> GetPorSolicitudAsync(uint solicitudId, CancellationToken ct);
	Task<DocumentoItemDto?> GetByIdAsync(uint id, CancellationToken ct);

	Task DeleteAsync(uint solicitudId, byte tipoDocumentoId, uint usuarioId, CancellationToken ct);
	Task<string?> GetRutaAsync(uint solicitudId, byte tipoDocumentoId, CancellationToken ct);
}
