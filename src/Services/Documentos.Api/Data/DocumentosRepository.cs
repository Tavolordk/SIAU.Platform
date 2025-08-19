// Documentos.Api/Data/DocumentosRepository.cs
using System.Data;
using System.Data.Common;
using Dapper;
using Documentos.Api.Models;
using SharedKernel.Abstractions;

namespace Documentos.Api.Data;

public sealed class DocumentosRepository(IConnectionFactory factory) : IDocumentosRepository
{
	private async Task<T> WithConn<T>(Func<IDbConnection, Task<T>> work, CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();
		return await work(conn);
	}

	public Task<uint> UpsertDocumentoAsync(
		uint solicitudId, byte tipoDocumentoId,
		string nombreOriginal, string? extension, string? mimeType, int tamanoBytes,
		string storageProveedor, string storageRuta, string checksumSha256,
		uint usuarioId, CancellationToken ct)
		=> WithConn<uint>(async conn =>
		{
			var p = new DynamicParameters();
			p.Add("p_solicitud_id", solicitudId);
			p.Add("p_tipo_documento_id", tipoDocumentoId);
			p.Add("p_nombre_original", nombreOriginal);
			p.Add("p_extension", extension);
			p.Add("p_mime_type", mimeType);
			p.Add("p_tamano_bytes", tamanoBytes);
			p.Add("p_storage_proveedor", storageProveedor);
			p.Add("p_storage_ruta", storageRuta);
			p.Add("p_checksum_sha256", checksumSha256);
			p.Add("p_usuario_id", usuarioId);

			// No regresa ID; hacemos upsert y luego leemos el ID
			await conn.ExecuteAsync(
				"CALL sp_adjuntar_documento(@p_solicitud_id,@p_tipo_documento_id,@p_nombre_original,@p_extension,@p_mime_type,@p_tamano_bytes,@p_storage_proveedor,@p_storage_ruta,@p_checksum_sha256,@p_usuario_id);",
				p);

			var id = await conn.ExecuteScalarAsync<uint>(
				"SELECT id FROM documentos WHERE solicitud_id=@sid AND tipo_documento_id=@td",
				new { sid = solicitudId, td = tipoDocumentoId });

			return id;
		}, ct);

	public Task<IReadOnlyList<DocumentoItemDto>> GetPorSolicitudAsync(uint solicitudId, CancellationToken ct)
		=> WithConn<IReadOnlyList<DocumentoItemDto>>(async conn =>
		{
			var rows = await conn.QueryAsync<DocumentoItemDto>(
				@"SELECT  id               AS Id,
                          solicitud_id     AS SolicitudId,
                          tipo_documento_id AS TipoDocumentoId,
                          nombre_original  AS NombreOriginal,
                          extension        AS Extension,
                          mime_type        AS MimeType,
                          tamano_bytes     AS TamanoBytes,
                          storage_proveedor AS StorageProveedor,
                          storage_ruta     AS StorageRuta,
                          checksum_sha256  AS ChecksumSha256,
                          fecha_carga      AS FechaCarga
                  FROM documentos
                  WHERE solicitud_id=@sol
                  ORDER BY id DESC",
				new { sol = solicitudId });
			return rows.ToList();
		}, ct);

	public Task<DocumentoItemDto?> GetByIdAsync(uint id, CancellationToken ct)
		=> WithConn(conn => conn.QuerySingleOrDefaultAsync<DocumentoItemDto>(
			@"SELECT  id               AS Id,
                      solicitud_id     AS SolicitudId,
                      tipo_documento_id AS TipoDocumentoId,
                      nombre_original  AS NombreOriginal,
                      extension        AS Extension,
                      mime_type        AS MimeType,
                      tamano_bytes     AS TamanoBytes,
                      storage_proveedor AS StorageProveedor,
                      storage_ruta     AS StorageRuta,
                      checksum_sha256  AS ChecksumSha256,
                      fecha_carga      AS FechaCarga
              FROM documentos WHERE id=@id", new { id }), ct);

	public Task<string?> GetRutaAsync(uint solicitudId, byte tipoDocumentoId, CancellationToken ct)
		=> WithConn(conn => conn.ExecuteScalarAsync<string?>(
			"SELECT storage_ruta FROM documentos WHERE solicitud_id=@s AND tipo_documento_id=@t",
			new { s = solicitudId, t = tipoDocumentoId }), ct);

	public Task DeleteAsync(uint solicitudId, byte tipoDocumentoId, uint usuarioId, CancellationToken ct)
		=> WithConn(conn => conn.ExecuteAsync(
			"CALL sp_eliminar_documento(@p_solicitud_id,@p_tipo_documento_id,@p_usuario_id)",
			new { p_solicitud_id = solicitudId, p_tipo_documento_id = tipoDocumentoId, p_usuario_id = usuarioId }), ct);
}
